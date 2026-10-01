using Microsoft.AspNetCore.Mvc;
using Syntech.WebApp.Models;
using System.Diagnostics;

namespace Syntech.WebApp.Controllers
{
    /// <summary>
    /// Handles requests for the application's public home pages.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Initializes a new instance of the HomeController.
        /// </summary>
        /// <param name="logger">
        /// ASP.NET Core logging service used to record application events
        /// and unexpected errors.
        /// </param>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Services()
        {
            return View();
        }

        public IActionResult Portfolio()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Displays the standard application error page.
        ///
        /// The request ID is included to help developers identify the
        /// corresponding request in application logs.
        /// </summary>
        /// <returns>
        /// The Home/Error view containing the error information.
        /// </returns>
        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id
                        ?? HttpContext.TraceIdentifier
                }
            );
        }
    }
}

