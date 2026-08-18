using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Syntech.WebApp.Models;
using Syntech.WebApp.Services;

namespace Syntech.WebApp.Controllers
{
    /// <summary>
    /// Handles security-related operations for the currently authenticated
    /// user.
    ///
    /// Current responsibilities include:
    ///
    /// - Changing the authenticated user's password.
    ///
    /// Security design:
    ///
    /// The controller never accepts a UserId from the browser to determine
    /// which account is being modified.
    ///
    /// Instead, the UserId is obtained from the authenticated user's
    /// NameIdentifier claim.
    /// </summary>
    [Authorize]
    public class AccountSecurityController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<AccountSecurityController> _logger;


        /// <summary>
        /// Initializes a new instance of AccountSecurityController.
        /// </summary>
        /// <param name="authenticationService">
        /// Authentication service responsible for password verification
        /// and password hashing.
        /// </param>
        /// <param name="logger">
        /// ASP.NET Core logging service.
        /// </param>
        public AccountSecurityController(
            IAuthenticationService authenticationService,
            ILogger<AccountSecurityController> logger)
        {
            _authenticationService = authenticationService;
            _logger = logger;
        }


        // =====================================================================
        // CHANGE PASSWORD
        // =====================================================================

        /// <summary>
        /// Displays the change-password form.
        ///
        /// GET:
        /// /AccountSecurity/ChangePassword
        /// </summary>
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }


        /// <summary>
        /// Processes a submitted password-change request.
        ///
        /// The operation:
        ///
        /// 1. Validates the submitted model.
        /// 2. Obtains the authenticated user's ID from the authentication
        ///    claim.
        /// 3. Verifies the current password.
        /// 4. Generates a secure hash for the new password.
        /// 5. Stores the new hash in dbo.User.
        ///
        /// The controller never stores or logs plain-text passwords.
        ///
        /// POST:
        /// /AccountSecurity/ChangePassword
        /// </summary>
        /// <param name="model">
        /// Password-change form data.
        /// </param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordViewModel model)
        {
            // -------------------------------------------------------------
            // Validate the submitted model.
            //
            // This includes:
            //
            // - Current password required.
            // - New password required.
            // - New password between 8 and 100 characters.
            // - Confirmation must match the new password.
            // -------------------------------------------------------------

            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // -------------------------------------------------------------
            // Obtain the authenticated user's database ID.
            //
            // We deliberately do NOT use a UserId supplied by the form.
            // -------------------------------------------------------------

            string? userIdClaim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);


            if (!int.TryParse(
                    userIdClaim,
                    out int userId))
            {
                _logger.LogWarning(
                    "Authenticated user has an invalid UserId claim " +
                    "during password change.");

                return Forbid();
            }


            try
            {
                // ---------------------------------------------------------
                // Ask AuthenticationService to perform the password
                // verification and password hashing.
                // ---------------------------------------------------------

                bool passwordChanged =
                    await _authenticationService
                        .ChangePasswordAsync(
                            userId,
                            model.CurrentPassword,
                            model.NewPassword);


                // ---------------------------------------------------------
                // A false result means the current password was incorrect
                // or the user account could not be found.
                //
                // We deliberately use a generic message rather than
                // exposing account-specific information.
                // ---------------------------------------------------------

                if (!passwordChanged)
                {
                    ModelState.AddModelError(
                        nameof(model.CurrentPassword),
                        "The current password is incorrect.");

                    return View(model);
                }


                // ---------------------------------------------------------
                // Password change succeeded.
                // ---------------------------------------------------------

                _logger.LogInformation(
                    "User {UserId} successfully changed their password.",
                    userId);


                // ---------------------------------------------------------
                // Store a temporary success message.
                // -------------------------------------------------------------

                TempData["PasswordSuccess"] =
                    "Your password was changed successfully.";


                // ---------------------------------------------------------
                // Redirect after POST.
                //
                // This prevents accidental form resubmission when the
                // browser is refreshed.
                // -------------------------------------------------------------

                return RedirectToAction(
                    nameof(ChangePassword));
            }
            catch (InvalidOperationException ex)
            {
                // ---------------------------------------------------------
                // Handle known business-rule errors, such as attempting
                // to reuse the current password.
                // ---------------------------------------------------------

                _logger.LogWarning(
                    ex,
                    "Password change rejected for user {UserId}.",
                    userId);

                ModelState.AddModelError(
                    nameof(model.NewPassword),
                    ex.Message);

                return View(model);
            }
            catch (Exception ex)
            {
                // ---------------------------------------------------------
                // Handle unexpected errors without exposing technical
                // implementation details to the user.
                // ---------------------------------------------------------

                _logger.LogError(
                    ex,
                    "Unexpected error while changing password for " +
                    "user {UserId}.",
                    userId);

                ModelState.AddModelError(
                    string.Empty,
                    "We could not change your password at this time. " +
                    "Please try again later.");

                return View(model);
            }
        }
    }
}