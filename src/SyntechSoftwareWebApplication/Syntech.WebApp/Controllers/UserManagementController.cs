using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syntech.WebApp.Data;
using Syntech.WebApp.Models;
using Syntech.WebApp.Services;

namespace Syntech.WebApp.Controllers
{
    /// <summary>
    /// Provides administrator-only user-management functionality.
    ///
    /// Current operations include:
    ///
    /// - Viewing users.
    /// - Creating users.
    /// - Editing users.
    /// - Activating users.
    /// - Deactivating users.
    /// - Deleting users.
    ///
    /// All actions are protected by the Admin role requirement.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IAuthenticationService
            _authenticationService;

        private readonly ILogger<UserManagementController>
            _logger;


        /// <summary>
        /// Initializes the controller.
        /// </summary>
        public UserManagementController(
            ApplicationDbContext context,
            IAuthenticationService authenticationService,
            ILogger<UserManagementController> logger)
        {
            _context = context;
            _authenticationService = authenticationService;
            _logger = logger;
        }


        // =====================================================================
        // LIST USERS
        // =====================================================================

        /// <summary>
        /// Displays all registered users.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<UserManagementViewModel> users =
                await _context.Users
                    .AsNoTracking()
                    .OrderBy(user => user.UserId)
                    .Select(
                        user =>
                            new UserManagementViewModel
                            {
                                UserId = user.UserId,
                                Name = user.Name,
                                Email = user.Email,
                                Role = user.Role,
                                IsActive = user.IsActive
                            })
                    .ToListAsync();


            return View(users);
        }


        // =====================================================================
        // CREATE USER
        // =====================================================================

        /// <summary>
        /// Displays the Create User form.
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        /// <summary>
        /// Creates a new user on behalf of an administrator.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            AdminCreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            try
            {
                User user =
                    await _authenticationService
                        .CreateUserByAdminAsync(
                            model.Name,
                            model.Email,
                            model.Password,
                            model.Role);


                _logger.LogInformation(
                    "Administrator created user {UserId} " +
                    "with role {Role}.",
                    user.UserId,
                    user.Role);


                TempData["UserManagementSuccess"] =
                    $"User '{user.Name}' was created successfully.";


                return RedirectToAction(
                    nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    ex.Message);

                return View(model);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                return View(model);
            }
        }


        // =====================================================================
        // EDIT USER
        // =====================================================================

        /// <summary>
        /// Displays the Edit User form.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            User? user =
                await _context.Users
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        existingUser =>
                            existingUser.UserId == id);


            if (user == null)
            {
                return NotFound();
            }


            var model =
                new AdminEditUserViewModel
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                };


            return View(model);
        }


        /// <summary>
        /// Processes the Edit User form.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            AdminEditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            try
            {
                User user =
                    await _authenticationService
                        .UpdateUserByAdminAsync(
                            model.UserId,
                            model.Name,
                            model.Email,
                            model.Role);


                _logger.LogInformation(
                    "Administrator updated user {UserId}.",
                    user.UserId);


                TempData["UserManagementSuccess"] =
                    $"User '{user.Name}' was updated successfully.";


                return RedirectToAction(
                    nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    ex.Message);

                return View(model);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    ex.Message);

                return View(model);
            }
        }


        // =====================================================================
        // ACTIVATE / DEACTIVATE
        // =====================================================================

        /// <summary>
        /// Activates or deactivates a user account.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetActiveStatus(
            int id,
            bool isActive)
        {
            try
            {
                User user =
                    await _authenticationService
                        .SetUserActiveStatusAsync(
                            id,
                            isActive);


                string status =
                    isActive
                        ? "activated"
                        : "deactivated";


                _logger.LogInformation(
                    "Administrator {Status} user {UserId}.",
                    status,
                    user.UserId);


                TempData["UserManagementSuccess"] =
                    $"User '{user.Name}' was {status} successfully.";


                return RedirectToAction(
                    nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }


        // =====================================================================
        // DELETE USER - CONFIRMATION PAGE
        // =====================================================================

        /// <summary>
        /// Displays a confirmation page before permanently deleting a user.
        ///
        /// An administrator cannot delete their own account.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Delete(
            int id)
        {
            int currentAdminUserId =
                GetCurrentUserId();


            if (id == currentAdminUserId)
            {
                TempData["UserManagementError"] =
                    "You cannot delete your own administrator account.";

                return RedirectToAction(
                    nameof(Index));
            }


            User? user =
                await _context.Users
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        existingUser =>
                            existingUser.UserId == id);


            if (user == null)
            {
                return NotFound();
            }


            var model =
                new DeleteUserViewModel
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                };


            return View(model);
        }


        // =====================================================================
        // DELETE USER - PROCESS
        // =====================================================================

        /// <summary>
        /// Permanently deletes a user after the administrator confirms
        /// the deletion.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            int currentAdminUserId =
                GetCurrentUserId();


            try
            {
                await _authenticationService
                    .DeleteUserByAdminAsync(
                        id,
                        currentAdminUserId);


                _logger.LogInformation(
                    "Administrator deleted user {UserId}.",
                    id);


                TempData["UserManagementSuccess"] =
                    "The user was deleted successfully.";


                return RedirectToAction(
                    nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["UserManagementError"] =
                    ex.Message;

                return RedirectToAction(
                    nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }


        // =====================================================================
        // CURRENT USER ID
        // =====================================================================

        /// <summary>
        /// Retrieves the database ID of the currently authenticated user
        /// from the NameIdentifier claim.
        /// </summary>
        private int GetCurrentUserId()
        {
            string? userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);


            if (!int.TryParse(
                    userIdValue,
                    out int userId))
            {
                throw new InvalidOperationException(
                    "The authenticated user's ID could not be determined.");
            }


            return userId;
        }
    }
}