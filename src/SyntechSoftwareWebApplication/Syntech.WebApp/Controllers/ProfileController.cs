using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syntech.WebApp.Data;
using Syntech.WebApp.Models;

namespace Syntech.WebApp.Controllers
{
    /// <summary>
    /// Handles the authenticated user's own profile.
    ///
    /// Security design:
    ///
    /// The controller never trusts a UserId supplied by the browser to
    /// determine which account should be accessed or modified.
    ///
    /// Instead, the authenticated user's UserId is obtained from the
    /// NameIdentifier claim contained in the authentication cookie.
    ///
    /// This prevents a user from changing another user's profile by
    /// manipulating a UserId value.
    /// </summary>
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProfileController> _logger;


        /// <summary>
        /// Initializes a new instance of ProfileController.
        /// </summary>
        public ProfileController(
            ApplicationDbContext context,
            ILogger<ProfileController> logger)
        {
            _context = context;
            _logger = logger;
        }


        // =====================================================================
        // VIEW PROFILE
        // =====================================================================

        /// <summary>
        /// Displays the currently authenticated user's profile.
        ///
        /// GET:
        /// /Profile
        ///
        /// The user ID is obtained from the authentication claim rather than
        /// from a route parameter or query string.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // -------------------------------------------------------------
            // Obtain the authenticated user's ID from the NameIdentifier
            // claim.
            // -------------------------------------------------------------

            if (!TryGetAuthenticatedUserId(out int userId))
            {
                return Forbid();
            }


            // -------------------------------------------------------------
            // Load the authenticated user's record.
            // -------------------------------------------------------------

            User? user = await _context.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    existingUser =>
                        existingUser.UserId == userId);


            // -------------------------------------------------------------
            // If the authenticated account no longer exists, terminate
            // the authentication session.
            // -------------------------------------------------------------

            if (user == null)
            {
                _logger.LogWarning(
                    "Authenticated user {UserId} could not be found " +
                    "in the database.",
                    userId);

                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults
                        .AuthenticationScheme);

                return RedirectToAction(
                    "Login",
                    "Account");
            }


            // -------------------------------------------------------------
            // Convert the database entity into a ViewModel.
            //
            // We deliberately do not pass the User entity directly to
            // the Razor view.
            // -------------------------------------------------------------

            ProfileViewModel model = CreateProfileViewModel(user);

            return View(model);
        }


        // =====================================================================
        // EDIT PROFILE
        // =====================================================================

        /// <summary>
        /// Updates the authenticated user's editable profile information.
        ///
        /// Currently, only the user's name can be changed.
        ///
        /// POST:
        /// /Profile/Edit
        ///
        /// Security:
        ///
        /// - Authentication is required.
        /// - An antiforgery token is required.
        /// - The authenticated user's ID comes from the authentication claim.
        /// - UserId, Email, and Role from the submitted form are ignored.
        /// - Only the Name property is updated.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            ProfileViewModel model)
        {
            // -------------------------------------------------------------
            // Validate the submitted model.
            //
            // ProfileViewModel contains validation rules for Name.
            // -------------------------------------------------------------

            if (!ModelState.IsValid)
            {
                // We still need the authenticated user's trusted information
                // when redisplaying the page.
                //
                // Do not trust UserId, Email, or Role from the submitted form.
                if (TryGetAuthenticatedUserId(out int invalidModelUserId))
                {
                    User? existingUser = await _context.Users
                        .AsNoTracking()
                        .SingleOrDefaultAsync(
                            user =>
                                user.UserId == invalidModelUserId);

                    if (existingUser != null)
                    {
                        ProfileViewModel displayModel =
                            CreateProfileViewModel(existingUser);

                        // Preserve the validation errors generated from the
                        // submitted Name field.
                        ModelState.Remove(nameof(ProfileViewModel.UserId));
                        ModelState.Remove(nameof(ProfileViewModel.Email));
                        ModelState.Remove(nameof(ProfileViewModel.Role));

                        return View("Index", displayModel);
                    }
                }

                return View("Index", model);
            }


            // -------------------------------------------------------------
            // Determine which user is actually being edited.
            //
            // This deliberately ignores model.UserId.
            // -------------------------------------------------------------

            if (!TryGetAuthenticatedUserId(out int userId))
            {
                return Forbid();
            }


            try
            {
                // ---------------------------------------------------------
                // Load the authenticated user's database record.
                // ---------------------------------------------------------

                User? user = await _context.Users
                    .SingleOrDefaultAsync(
                        existingUser =>
                            existingUser.UserId == userId);


                if (user == null)
                {
                    _logger.LogWarning(
                        "User {UserId} attempted to update a profile " +
                        "that no longer exists.",
                        userId);

                    await HttpContext.SignOutAsync(
                        CookieAuthenticationDefaults
                            .AuthenticationScheme);

                    return RedirectToAction(
                        "Login",
                        "Account");
                }


                // ---------------------------------------------------------
                // Only update the property that this form is allowed
                // to change.
                //
                // We deliberately do NOT update:
                //
                // user.UserId
                // user.Email
                // user.PasswordHash
                // user.Role
                // ---------------------------------------------------------

                string updatedName = model.Name.Trim();

                user.Name = updatedName;


                // ---------------------------------------------------------
                // Persist the change.
                // ---------------------------------------------------------

                await _context.SaveChangesAsync();


                _logger.LogInformation(
                    "User {UserId} successfully updated their profile.",
                    userId);


                // ---------------------------------------------------------
                // Inform the user that the update succeeded.
                // ---------------------------------------------------------

                TempData["ProfileSuccess"] =
                    "Your profile was updated successfully.";


                // ---------------------------------------------------------
                // Redirect after POST.
                //
                // This follows the Post/Redirect/Get pattern and prevents
                // the browser from resubmitting the POST if the user
                // refreshes the page.
                // ---------------------------------------------------------

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                // ---------------------------------------------------------
                // Handle database update errors without exposing internal
                // database details to the user.
                // ---------------------------------------------------------

                _logger.LogError(
                    ex,
                    "Database error while updating profile for user {UserId}.",
                    userId);

                ModelState.AddModelError(
                    string.Empty,
                    "We could not save your profile changes. " +
                    "Please try again later.");

                return await ReloadProfileViewAsync(userId);
            }
            catch (Exception ex)
            {
                // ---------------------------------------------------------
                // Handle unexpected errors.
                // ---------------------------------------------------------

                _logger.LogError(
                    ex,
                    "Unexpected error while updating profile for user {UserId}.",
                    userId);

                ModelState.AddModelError(
                    string.Empty,
                    "An unexpected error occurred while updating your profile.");

                return await ReloadProfileViewAsync(userId);
            }
        }


        // =====================================================================
        // HELPER METHODS
        // =====================================================================

        /// <summary>
        /// Attempts to retrieve the authenticated user's database ID from
        /// the NameIdentifier claim.
        /// </summary>
        /// <param name="userId">
        /// The authenticated user's database ID when the claim is valid.
        /// </param>
        /// <returns>
        /// True when a valid user ID could be obtained; otherwise false.
        /// </returns>
        private bool TryGetAuthenticatedUserId(
            out int userId)
        {
            string? userIdClaim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            return int.TryParse(
                userIdClaim,
                out userId);
        }


        /// <summary>
        /// Creates a ProfileViewModel from a database User entity.
        ///
        /// Keeping this mapping in one place makes it less likely that
        /// sensitive User properties will accidentally be exposed to
        /// the view.
        /// </summary>
        private static ProfileViewModel CreateProfileViewModel(
            User user)
        {
            return new ProfileViewModel
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };
        }


        /// <summary>
        /// Reloads the authenticated user's trusted profile information
        /// after a validation or database error.
        ///
        /// This prevents submitted UserId, Email, or Role values from being
        /// used when redisplaying the profile.
        /// </summary>
        private async Task<IActionResult> ReloadProfileViewAsync(
            int userId)
        {
            User? user = await _context.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    existingUser =>
                        existingUser.UserId == userId);

            if (user == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            ProfileViewModel displayModel =
                CreateProfileViewModel(user);

            return View(
                "Index",
                displayModel);
        }
    }
}