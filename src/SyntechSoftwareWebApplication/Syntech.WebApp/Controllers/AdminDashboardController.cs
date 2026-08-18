using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syntech.WebApp.Data;
using Syntech.WebApp.Models;

namespace Syntech.WebApp.Controllers
{
    /// <summary>
    /// Provides administrator dashboard functionality.
    ///
    /// Access is restricted to authenticated users whose role is Admin.
    ///
    /// The dashboard provides administrators with a high-level overview
    /// of the users currently registered in the Syntech system.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly ILogger<AdminDashboardController>
            _logger;


        /// <summary>
        /// Initializes the administrator dashboard controller.
        /// </summary>
        /// <param name="context">
        /// Entity Framework Core database context used to retrieve
        /// dashboard information.
        /// </param>
        /// <param name="logger">
        /// Application logging service.
        /// </param>
        public AdminDashboardController(
            ApplicationDbContext context,
            ILogger<AdminDashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }


        // =====================================================================
        // ADMIN DASHBOARD
        // =====================================================================

        /// <summary>
        /// Displays the administrator dashboard.
        ///
        /// The dashboard retrieves user statistics directly from the
        /// database so that administrators see current system information.
        ///
        /// GET:
        /// /AdminDashboard
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // -------------------------------------------------------------
            // Retrieve total user count.
            // -------------------------------------------------------------

            int totalUsers =
                await _context.Users.CountAsync();


            // -------------------------------------------------------------
            // Retrieve active user count.
            // -------------------------------------------------------------

            int activeUsers =
                await _context.Users
                    .CountAsync(
                        user =>
                            user.IsActive);


            // -------------------------------------------------------------
            // Retrieve deactivated user count.
            // -------------------------------------------------------------

            int deactivatedUsers =
                await _context.Users
                    .CountAsync(
                        user =>
                            !user.IsActive);


            // -------------------------------------------------------------
            // Retrieve role counts.
            // -------------------------------------------------------------

            int adminCount =
                await _context.Users
                    .CountAsync(
                        user =>
                            user.Role == "Admin");


            int developerCount =
                await _context.Users
                    .CountAsync(
                        user =>
                            user.Role == "Developer");


            int clientCount =
                await _context.Users
                    .CountAsync(
                        user =>
                            user.Role == "Client");


            // -------------------------------------------------------------
            // Retrieve recently created users.
            //
            // The existing database structure does not currently contain
            // a CreatedDate column, so UserId is used as the ordering
            // mechanism because userID is generated sequentially by SQL
            // Server's IDENTITY column.
            // -------------------------------------------------------------

            List<RecentUserViewModel> recentUsers =
                await _context.Users
                    .AsNoTracking()
                    .OrderByDescending(
                        user =>
                            user.UserId)
                    .Take(5)
                    .Select(
                        user =>
                            new RecentUserViewModel
                            {
                                UserId = user.UserId,
                                Name = user.Name,
                                Email = user.Email,
                                Role = user.Role,
                                IsActive = user.IsActive
                            })
                    .ToListAsync();


            // -------------------------------------------------------------
            // Build the dashboard ViewModel.
            // -------------------------------------------------------------

            var model =
                new AdminDashboardViewModel
                {
                    TotalUsers = totalUsers,
                    ActiveUsers = activeUsers,
                    DeactivatedUsers = deactivatedUsers,
                    AdminCount = adminCount,
                    DeveloperCount = developerCount,
                    ClientCount = clientCount,
                    RecentUsers = recentUsers
                };


            _logger.LogInformation(
                "Administrator dashboard loaded by user {UserId}.",
                User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)
                    ?.Value);


            return View(model);
        }
    }
}