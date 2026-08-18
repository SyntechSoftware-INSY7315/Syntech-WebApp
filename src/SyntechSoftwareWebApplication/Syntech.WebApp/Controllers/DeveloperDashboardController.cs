using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syntech.WebApp.Data;
using Syntech.WebApp.Models;

namespace Syntech.WebApp.Controllers
{
    /// <summary>
    /// Provides functionality for the Developer Dashboard.
    ///
    /// Access to this controller is restricted to authenticated
    /// users whose role is Developer.
    ///
    /// This controller forms the foundation for developer-specific
    /// functionality that will be implemented in subsequent steps.
    /// </summary>
    [Authorize(Roles = "Developer")]
    public class DeveloperDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly ILogger<DeveloperDashboardController>
            _logger;


        /// <summary>
        /// Initializes the Developer Dashboard controller.
        /// </summary>
        /// <param name="context">
        /// Entity Framework Core database context.
        /// </param>
        /// <param name="logger">
        /// Application logging service.
        /// </param>
        public DeveloperDashboardController(
            ApplicationDbContext context,
            ILogger<DeveloperDashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }


        // =====================================================================
        // DEVELOPER DASHBOARD
        // =====================================================================

        /// <summary>
        /// Displays the authenticated Developer Dashboard.
        ///
        /// The authenticated user's database record is retrieved using
        /// the UserId stored in the authentication claims.
        ///
        /// GET:
        /// /DeveloperDashboard
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // -------------------------------------------------------------
            // Retrieve the authenticated user's database ID.
            // -------------------------------------------------------------

            string? userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);


            if (!int.TryParse(
                    userIdValue,
                    out int userId))
            {
                _logger.LogWarning(
                    "Developer dashboard could not determine " +
                    "the authenticated user's ID.");

                return Forbid();
            }


            // -------------------------------------------------------------
            // Retrieve the Developer's User record.
            //
            // We use the User table because it contains the developer's
            // common account information such as name, email, and role.
            // -------------------------------------------------------------

            User? user =
                await _context.Users
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        existingUser =>
                            existingUser.UserId == userId &&
                            existingUser.Role == "Developer");


            if (user == null)
            {
                _logger.LogWarning(
                    "Developer dashboard requested for invalid " +
                    "Developer account. User ID: {UserId}",
                    userId);

                return Forbid();
            }


            // -------------------------------------------------------------
            // Build the dashboard ViewModel.
            // -------------------------------------------------------------

            var model =
                new DeveloperDashboardViewModel
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                };


            _logger.LogInformation(
                "Developer dashboard loaded for user {UserId}.",
                user.UserId);


            return View(model);
        }
    }
}