using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Syntech.WebApp.Models;
using Syntech.WebApp.Services;

// Alias our application's authentication service interface.
//
// Microsoft.AspNetCore.Authentication also contains a type named
// IAuthenticationService, so the alias prevents an ambiguous reference.
using AppAuthenticationService = Syntech.WebApp.Services.IAuthenticationService;

namespace Syntech.WebApp.Controllers
{
    /// <summary>
    /// Handles authentication and account-related operations.
    ///
    /// Responsibilities currently include:
    ///
    /// - User registration
    /// - User login
    /// - User logout
    /// - Access-denied handling
    ///
    /// Additional account-management functionality such as profile updates
    /// and password changes will be added in later steps.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly AppAuthenticationService _authenticationService;
        private readonly ILogger<AccountController> _logger;


        /// <summary>
        /// Initializes a new instance of AccountController.
        /// </summary>
        /// <param name="authenticationService">
        /// Authentication service responsible for user lookup, registration,
        /// password hashing, and password verification.
        /// </param>
        /// <param name="logger">
        /// ASP.NET Core logging service.
        /// </param>
        public AccountController(
    AppAuthenticationService authenticationService,
    ILogger<AccountController> logger)
        {
            _authenticationService = authenticationService;
            _logger = logger;
        }


        // =====================================================================
        // REGISTRATION
        // =====================================================================

        /// <summary>
        /// Displays the user registration page.
        ///
        /// GET:
        /// /Account/Register
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        /// <summary>
        /// Processes a submitted registration form.
        ///
        /// Public registration is restricted to Client and Developer accounts.
        /// Administrator accounts must be created through the administrative
        /// user-management functionality.
        ///
        /// POST:
        /// /Account/Register
        /// </summary>
        /// <param name="model">
        /// Registration form data.
        /// </param>
        /// <returns>
        /// Registration page when validation fails, or the login page after
        /// successful registration.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model)
        {
            // -----------------------------------------------------------------
            // Validate the submitted model.
            // -----------------------------------------------------------------
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // -----------------------------------------------------------------
            // Public registration may only create Client and Developer
            // accounts.
            //
            // We deliberately do NOT trust a user-supplied "Admin" value.
            // Allowing public users to register as administrators would be a
            // critical authorization vulnerability.
            // -----------------------------------------------------------------

            string role = model.Role.Trim();

            if (!role.Equals(
                    "Client",
                    StringComparison.OrdinalIgnoreCase) &&
                !role.Equals(
                    "Developer",
                    StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "You may only register as a Client or Developer.");

                return View(model);
            }


            // -----------------------------------------------------------------
            // Normalise the role so that the value stored in SQL Server uses
            // the exact casing expected by the database.
            // -----------------------------------------------------------------

            role = role.Equals(
                "Client",
                StringComparison.OrdinalIgnoreCase)
                ? "Client"
                : "Developer";


            try
            {
                // -----------------------------------------------------------------
                // Check whether the email is already registered.
                //
                // The database also has a UNIQUE constraint on email. The
                // application-level check allows us to display a useful
                // validation message before attempting the database insert.
                // -----------------------------------------------------------------

                bool emailExists =
                    await _authenticationService
                        .EmailExistsAsync(model.Email);

                if (emailExists)
                {
                    ModelState.AddModelError(
                        nameof(model.Email),
                        "An account with this email address already exists.");

                    return View(model);
                }


                // -----------------------------------------------------------------
                // Create the account.
                //
                // AuthenticationService is responsible for hashing the
                // password before it reaches the database.
                // -----------------------------------------------------------------

                User user =
                    await _authenticationService.RegisterAsync(
                        model.Name,
                        model.Email,
                        model.Password,
                        role);


                _logger.LogInformation(
                    "User registration completed successfully. " +
                    "User ID: {UserId}, Role: {Role}",
                    user.UserId,
                    user.Role);


                // -----------------------------------------------------------------
                // Store a temporary success message.
                //
                // TempData survives the redirect to Login and is then removed
                // automatically after it has been read.
                // -----------------------------------------------------------------

                TempData["RegistrationSuccess"] =
                    "Your account was created successfully. " +
                    "You can now log in.";


                return RedirectToAction(nameof(Login));
            }
            catch (InvalidOperationException ex)
            {
                // -----------------------------------------------------------------
                // Handle known business-rule errors.
                // -----------------------------------------------------------------

                _logger.LogWarning(
                    ex,
                    "Registration failed for email {Email}.",
                    model.Email);

                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                return View(model);
            }
            catch (Exception ex)
            {
                // -----------------------------------------------------------------
                // Handle unexpected errors.
                //
                // We log the technical details but do not expose the exception
                // itself to the user.
                // -----------------------------------------------------------------

                _logger.LogError(
                    ex,
                    "Unexpected error occurred during registration.");

                ModelState.AddModelError(
                    string.Empty,
                    "We could not create your account at this time. " +
                    "Please try again later.");

                return View(model);
            }
        }


        // =====================================================================
        // LOGIN
        // =====================================================================

        /// <summary>
        /// Displays the login page.
        ///
        /// GET:
        /// /Account/Login
        /// </summary>
        [HttpGet]
        public IActionResult Login(
            string? returnUrl = null)
        {
            // Store a validated local return URL so that, after login,
            // the user can be returned to the page they originally requested.
            //
            // We only accept local URLs to prevent an attacker from using
            // the login endpoint as an open redirect.
            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                ViewData["ReturnUrl"] = returnUrl;
            }

            return View();
        }


        /// <summary>
        /// Processes a submitted login form.
        ///
        /// The method:
        ///
        /// 1. Validates the login form.
        /// 2. Finds the user by email.
        /// 3. Verifies the password against the stored password hash.
        /// 4. Creates authentication claims.
        /// 5. Adds the user's role as a role claim.
        /// 6. Creates the encrypted authentication cookie.
        /// 7. Redirects the user to the appropriate destination.
        ///
        /// POST:
        /// /Account/Login
        /// </summary>
        /// <param name="model">
        /// Login form data.
        /// </param>
        /// <param name="returnUrl">
        /// Optional local URL that the user originally attempted to access.
        /// </param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model,
            string? returnUrl = null)
        {
            // -----------------------------------------------------------------
            // Validate the login model.
            // -----------------------------------------------------------------

            if (!ModelState.IsValid)
            {
                return View(model);
            }


            try
            {
                // -----------------------------------------------------------------
                // Authenticate the user.
                //
                // AuthenticationService performs password verification using
                // PasswordHasher<User>.
                // -----------------------------------------------------------------

                User? user =
                    await _authenticationService.AuthenticateAsync(
                        model.Email,
                        model.Password);


                // -----------------------------------------------------------------
                // Failed authentication
                //
                // We deliberately use a generic message rather than saying
                // whether the email or password was incorrect.
                //
                // This reduces the information exposed to attackers trying
                // to discover which email addresses have accounts.
                // -----------------------------------------------------------------

                if (user == null)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Invalid email address or password.");

                    _logger.LogWarning(
                        "Failed login attempt for email {Email}.",
                        model.Email);

                    return View(model);
                }


                // -----------------------------------------------------------------
                // Create the user's claims.
                //
                // Claims are pieces of information that ASP.NET Core associates
                // with the authenticated user.
                // -----------------------------------------------------------------

                var claims = new List<Claim>
                {
                    // Unique database user ID.
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        user.UserId.ToString()),

                    // Display name.
                    new Claim(
                        ClaimTypes.Name,
                        user.Name),

                    // Email address.
                    new Claim(
                        ClaimTypes.Email,
                        user.Email),

                    // Role used by ASP.NET Core authorization.
                    new Claim(
                        ClaimTypes.Role,
                        user.Role)
                };


                // -----------------------------------------------------------------
                // Create the ClaimsIdentity.
                //
                // The authentication scheme must match the cookie authentication
                // scheme configured in Program.cs.
                // -----------------------------------------------------------------

                var identity =
                    new ClaimsIdentity(
                        claims,
                        CookieAuthenticationDefaults
                            .AuthenticationScheme);


                // -----------------------------------------------------------------
                // Create the ClaimsPrincipal.
                //
                // A ClaimsPrincipal represents the authenticated user for the
                // current HTTP request.
                // -----------------------------------------------------------------

                var principal =
                    new ClaimsPrincipal(identity);


                // -----------------------------------------------------------------
                // Configure authentication properties.
                // -----------------------------------------------------------------

                var authenticationProperties =
                    new AuthenticationProperties
                    {
                        // Allows the user to choose whether the session should
                        // persist beyond the current browser session.
                        IsPersistent = model.RememberMe,

                        // Refresh the authentication cookie automatically while
                        // the user remains active.
                        AllowRefresh = true
                    };


                // -----------------------------------------------------------------
                // Create the authentication cookie.
                //
                // ASP.NET Core encrypts and signs the authentication ticket
                // according to the configured cookie authentication system.
                //
                // The password is NOT stored in the cookie.
                // -----------------------------------------------------------------

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults
                        .AuthenticationScheme,
                    principal,
                    authenticationProperties);


                _logger.LogInformation(
                    "User {UserId} logged in successfully.",
                    user.UserId);


                // -----------------------------------------------------------------
                // Return the user to their original local destination when
                // possible.
                // -----------------------------------------------------------------

                if (!string.IsNullOrWhiteSpace(returnUrl) &&
                    Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }


                // -----------------------------------------------------------------
                // Otherwise redirect according to the user's role.
                // -----------------------------------------------------------------

                // -----------------------------------------------------------------
                // Redirect the authenticated user to the dashboard corresponding
                // to their database role.
                //
                // The role has already been validated by the authentication service
                // and is also present in the authenticated user's Role claim.
                // -----------------------------------------------------------------

                return user.Role switch
                {
                    "Admin" =>
                        RedirectToAction(
                            "Index",
                            "AdminDashboard"),

                    "Developer" =>
                        RedirectToAction(
                            "Index",
                            "DeveloperDashboard"),

                    "Client" =>
                        RedirectToAction(
                            "Index",
                            "ClientDashboard"),

                    // Safe fallback for an unexpected role value.
                    _ =>
                        RedirectToAction(
                            "Index",
                            "Home")
                };
            }
            catch (Exception ex)
            {
                // -----------------------------------------------------------------
                // Handle unexpected technical errors without exposing sensitive
                // implementation details to the user.
                // -----------------------------------------------------------------

                _logger.LogError(
                    ex,
                    "Unexpected error occurred during login.");

                ModelState.AddModelError(
                    string.Empty,
                    "We could not complete the login at this time. " +
                    "Please try again later.");

                return View(model);
            }
        }


        // =====================================================================
        // LOGOUT
        // =====================================================================

        /// <summary>
        /// Logs the current user out.
        ///
        /// ASP.NET Core removes the authentication session by invalidating
        /// the authentication cookie.
        ///
        /// POST:
        /// /Account/Logout
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme);

            _logger.LogInformation(
                "User logged out successfully.");

            return RedirectToAction(
                "Index",
                "Home");
        }


        // =====================================================================
        // ACCESS DENIED
        // =====================================================================

        /// <summary>
        /// Displays the access-denied page when an authenticated user attempts
        /// to access a resource they are not authorised to access.
        ///
        /// GET:
        /// /Account/AccessDenied
        /// </summary>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

