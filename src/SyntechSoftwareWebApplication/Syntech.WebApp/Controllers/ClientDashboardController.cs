using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syntech.WebApp.Data;
using Syntech.WebApp.Models;

namespace Syntech.WebApp.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool TryGetUserId(out int userId)
        {
            userId = 0;
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out userId);
        }

        // GET: /ClientDashboard
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!TryGetUserId(out int userId))
                return Challenge();

            // ClientID on Project = User.UserId of the client
            var projects = await _context.Projects
                .Where(p => p.ClientID == userId)
                .OrderByDescending(p => p.Deadline)
                .Select(p => new ClientProjectListItemViewModel
                {
                    ProjectID = p.ProjectID,
                    ProjectName = p.ProjectName,
                    Status = p.Status,
                    ProgressPercentage = p.ProgressPercentage,
                    StartDate = p.StartDate,
                    Deadline = p.Deadline
                })
                .ToListAsync();

            return View(projects);
        }

        // GET: /ClientDashboard/ProjectDetails/5
        [HttpGet]
        public async Task<IActionResult> ProjectDetails(int id)
        {
            if (!TryGetUserId(out int userId))
                return Challenge();

            var project = await _context.Projects
                .Include(p => p.ProjectUpdates)
                    .ThenInclude(u => u.User)
                .FirstOrDefaultAsync(p => p.ProjectID == id && p.ClientID == userId);

            if (project == null)
                return NotFound(); // not theirs or doesn't exist

            project.ProjectUpdates = project.ProjectUpdates
                .OrderByDescending(u => u.CreatedDate)
                .ToList();

            return View(project); // or map to ClientProjectDetailsViewModel
        }
    }
}