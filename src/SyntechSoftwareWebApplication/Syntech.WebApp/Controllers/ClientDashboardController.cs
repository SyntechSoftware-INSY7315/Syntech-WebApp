using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Syntech.WebApp.Controllers
{
    /// <summary>
    /// Provides dashboard functionality for authenticated clients.
    ///
    /// Access is restricted using ASP.NET Core role-based authorization.
    /// Only users whose role claim is "Client" can access this controller.
    /// </summary>
    [Authorize(Roles = "Client")]
    public class ClientDashboardController : Controller
    {
        /// <summary>
        /// Displays the client dashboard.
        ///
        /// Only authenticated users with the Client role can reach
        /// this action.
        /// </summary>
        /// <returns>
        /// The client dashboard view.
        /// </returns>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}