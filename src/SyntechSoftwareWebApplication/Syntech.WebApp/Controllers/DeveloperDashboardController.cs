using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syntech.WebApp.Data;
using Syntech.WebApp.Models;

namespace Syntech.WebApp.Controllers
{
    /// <summary>
    /// Developer dashboard: assigned projects and progress updates only.
    /// </summary>
    [Authorize(Roles = "Developer")]
    public class DeveloperDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DeveloperDashboardController> _logger;

        public DeveloperDashboardController(
            ApplicationDbContext context,
            ILogger<DeveloperDashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private bool TryGetAuthenticatedUserId(out int userId)
        {
            userId = 0;
            string? value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out userId);
        }

        private async Task<bool> IsAssignedToProjectAsync(int projectId, int developerUserId)
        {
            return await _context.DeveloperProjects
                .AnyAsync(dp =>
                    dp.ProjectID == projectId &&
                    dp.DeveloperID == developerUserId);
        }

        // ---------------------------------------------------------------------
        // GET: /DeveloperDashboard
        // ---------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!TryGetAuthenticatedUserId(out int userId))
            {
                return Challenge();
            }

            User? user = await _context.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(u =>
                    u.UserId == userId &&
                    u.Role == "Developer");

            if (user == null)
            {
                _logger.LogWarning(
                    "Developer dashboard requested for invalid Developer account. User ID: {UserId}",
                    userId);
                return Forbid();
            }

            List<DeveloperProjectListItemViewModel> projects =
                await _context.DeveloperProjects
                    .AsNoTracking()
                    .Where(dp => dp.DeveloperID == userId)
                    .Select(dp => dp.Project!)
                    .Where(p => p != null)
                    .OrderByDescending(p => p.Deadline)
                    .Select(p => new DeveloperProjectListItemViewModel
                    {
                        ProjectID = p.ProjectID,
                        ProjectName = p.ProjectName,
                        Status = p.Status,
                        ProgressPercentage = p.ProgressPercentage,
                        StartDate = p.StartDate,
                        Deadline = p.Deadline
                    })
                    .ToListAsync();

            ViewBag.DeveloperName = user.Name;

            _logger.LogInformation(
                "Developer dashboard loaded for user {UserId} with {Count} projects.",
                userId,
                projects.Count);

            return View(projects);
        }

        // ---------------------------------------------------------------------
        // GET: /DeveloperDashboard/ProjectDetails/5
        // ---------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> ProjectDetails(int id)
        {
            if (!TryGetAuthenticatedUserId(out int userId))
            {
                return Challenge();
            }

            if (!await IsAssignedToProjectAsync(id, userId))
            {
                return Forbid();
            }

            Project? project = await _context.Projects
                .Include(p => p.ProjectUpdates)
                    .ThenInclude(u => u.User)
                .FirstOrDefaultAsync(p => p.ProjectID == id);

            if (project == null)
            {
                return NotFound();
            }

            project.ProjectUpdates = project.ProjectUpdates
                .OrderByDescending(u => u.CreatedDate)
                .ToList();

            return View(project);
        }

        // ---------------------------------------------------------------------
        // GET: /DeveloperDashboard/CreateUpdate?projectId=5
        // ---------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> CreateUpdate(int projectId)
        {
            if (!TryGetAuthenticatedUserId(out int userId))
            {
                return Challenge();
            }

            if (!await IsAssignedToProjectAsync(projectId, userId))
            {
                return Forbid();
            }

            Project? project = await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProjectID == projectId);

            if (project == null)
            {
                return NotFound();
            }

            ViewBag.ProjectName = project.ProjectName;

            return View(new ProjectUpdate
            {
                ProjectID = projectId
            });
        }

        // ---------------------------------------------------------------------
        // POST: /DeveloperDashboard/CreateUpdate
        // ---------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUpdate(ProjectUpdate model)
        {
            if (!TryGetAuthenticatedUserId(out int userId))
            {
                return Challenge();
            }

            if (!await IsAssignedToProjectAsync(model.ProjectID, userId))
            {
                return Forbid();
            }

            Project? project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectID == model.ProjectID);

            if (project == null)
            {
                return NotFound();
            }

            ViewBag.ProjectName = project.ProjectName;

            // Bind server-side fields (never trust client for these)
            model.UserID = userId;
            model.CreatedDate = DateTime.Now;
            model.UpdateID = 0;

            // Avoid validating navigation properties
            ModelState.Remove(nameof(ProjectUpdate.Project));
            ModelState.Remove(nameof(ProjectUpdate.User));
            ModelState.Remove(nameof(ProjectUpdate.UserID));
            ModelState.Remove(nameof(ProjectUpdate.CreatedDate));
            ModelState.Remove(nameof(ProjectUpdate.UpdateID));

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.ProgressPercentage.HasValue)
            {
                project.ProgressPercentage = model.ProgressPercentage.Value;
            }

            _context.ProjectUpdates.Add(model);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Developer {UserId} added update to project {ProjectId}.",
                userId,
                model.ProjectID);

            return RedirectToAction(nameof(ProjectDetails), new { id = model.ProjectID });
        }

        // ---------------------------------------------------------------------
        // GET: /DeveloperDashboard/EditUpdate/5
        // ---------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> EditUpdate(int id)
        {
            if (!TryGetAuthenticatedUserId(out int userId))
            {
                return Challenge();
            }

            ProjectUpdate? update = await _context.ProjectUpdates
                .FirstOrDefaultAsync(u => u.UpdateID == id);

            if (update == null)
            {
                return NotFound();
            }

            // Must be assigned to the project
            if (!await IsAssignedToProjectAsync(update.ProjectID, userId))
            {
                return Forbid();
            }

            // May only edit own notes
            if (update.UserID != userId)
            {
                return Forbid();
            }

            Project? project = await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProjectID == update.ProjectID);

            ViewBag.ProjectName = project?.ProjectName ?? "Project";

            return View(update);
        }

        // ---------------------------------------------------------------------
        // POST: /DeveloperDashboard/EditUpdate
        // ---------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUpdate(int id, ProjectUpdate model)
        {
            if (!TryGetAuthenticatedUserId(out int userId))
            {
                return Challenge();
            }

            if (id != model.UpdateID)
            {
                return BadRequest();
            }

            ProjectUpdate? existing = await _context.ProjectUpdates
                .FirstOrDefaultAsync(u => u.UpdateID == id);

            if (existing == null)
            {
                return NotFound();
            }

            if (!await IsAssignedToProjectAsync(existing.ProjectID, userId))
            {
                return Forbid();
            }

            if (existing.UserID != userId)
            {
                return Forbid();
            }

            Project? project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectID == existing.ProjectID);

            if (project == null)
            {
                return NotFound();
            }

            ViewBag.ProjectName = project.ProjectName;

            ModelState.Remove(nameof(ProjectUpdate.Project));
            ModelState.Remove(nameof(ProjectUpdate.User));
            ModelState.Remove(nameof(ProjectUpdate.UserID));
            ModelState.Remove(nameof(ProjectUpdate.CreatedDate));
            ModelState.Remove(nameof(ProjectUpdate.ProjectID));

            if (!ModelState.IsValid)
            {
                model.ProjectID = existing.ProjectID;
                model.UserID = existing.UserID;
                model.CreatedDate = existing.CreatedDate;
                return View(model);
            }

            existing.UpdateTitle = model.UpdateTitle;
            existing.UpdateDescription = model.UpdateDescription;
            existing.ProgressPercentage = model.ProgressPercentage;
            // Do not change UserID, ProjectID, or CreatedDate

            if (model.ProgressPercentage.HasValue)
            {
                project.ProgressPercentage = model.ProgressPercentage.Value;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Developer {UserId} edited update {UpdateId} on project {ProjectId}.",
                userId,
                existing.UpdateID,
                existing.ProjectID);

            return RedirectToAction(nameof(ProjectDetails), new { id = existing.ProjectID });
        }

        // ---------------------------------------------------------------------
        // GET: /DeveloperDashboard/DeleteUpdate/5
        // ---------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> DeleteUpdate(int id)
        {
            if (!TryGetAuthenticatedUserId(out int userId))
            {
                return Challenge();
            }

            ProjectUpdate? update = await _context.ProjectUpdates
                .Include(u => u.User)
                .FirstOrDefaultAsync(u => u.UpdateID == id);

            if (update == null)
            {
                return NotFound();
            }

            if (!await IsAssignedToProjectAsync(update.ProjectID, userId))
            {
                return Forbid();
            }

            if (update.UserID != userId)
            {
                return Forbid();
            }

            Project? project = await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProjectID == update.ProjectID);

            ViewBag.ProjectName = project?.ProjectName ?? "Project";

            return View(update);
        }

        // ---------------------------------------------------------------------
        // POST: /DeveloperDashboard/DeleteUpdate
        // ---------------------------------------------------------------------
        [HttpPost, ActionName("DeleteUpdate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUpdateConfirmed(int id)
        {
            if (!TryGetAuthenticatedUserId(out int userId))
            {
                return Challenge();
            }

            ProjectUpdate? update = await _context.ProjectUpdates
                .FirstOrDefaultAsync(u => u.UpdateID == id);

            if (update == null)
            {
                return NotFound();
            }

            if (!await IsAssignedToProjectAsync(update.ProjectID, userId))
            {
                return Forbid();
            }

            if (update.UserID != userId)
            {
                return Forbid();
            }

            int projectId = update.ProjectID;

            _context.ProjectUpdates.Remove(update);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Developer {UserId} deleted update {UpdateId} on project {ProjectId}.",
                userId,
                id,
                projectId);

            return RedirectToAction(nameof(ProjectDetails), new { id = projectId });
        }
    }
}