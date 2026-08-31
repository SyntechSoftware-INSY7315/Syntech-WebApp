using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Syntech.WebApp.Data;
using Syntech.WebApp.Models;
using System.Security.Claims;


namespace Syntech.WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
        string searchString,
        string statusFilter,
        int? clientFilter)
        {
            var projects = _context.Projects
                .AsQueryable();

            // Search by project name
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                projects = projects.Where(p =>
                    p.ProjectName.Contains(searchString));
            }

            // Filter by status
            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                projects = projects.Where(p =>
                    p.Status == statusFilter);
            }

            // Filter by client
            if (clientFilter.HasValue)
            {
                projects = projects.Where(p =>
                    p.ClientID == clientFilter.Value);
            }

            // Get clients for the dropdown
            ViewBag.Clients = await _context.Clients
                .Include(c => c.User)
                .ToListAsync();

            // Keep the selected values in the form
            ViewBag.SearchString = searchString;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.ClientFilter = clientFilter;

            return View(await projects.ToListAsync());
        }

        // GET: Project/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Developers = await _context.Developers
                .Include(d => d.User)
                .ToListAsync();

            var clients = await _context.Clients
                .Include(c => c.User)
                .ToListAsync();

            var admins = await _context.Admins
                .Include(a => a.User)
                .ToListAsync();

            ViewBag.Clients = clients.Select(c => new SelectListItem
            {
                Value = c.UserId.ToString(),
                Text = c.User != null ? c.User.Name : "Unknown Client"
            }).ToList();

            ViewBag.Admins = admins.Select(a => new SelectListItem
            {
                Value = a.UserId.ToString(),
                Text = a.User != null ? a.User.Name : "Unknown Admin"
            }).ToList();

            return View();
        }

        // POST: Project/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project)
        {
            if (ModelState.IsValid)
            {
                _context.Add(project);

                await _context.SaveChangesAsync();

                foreach (var developerID in project.SelectedDeveloperIDs)
                {
                    var developerProject = new DeveloperProject
                    {
                        ProjectID = project.ProjectID,
                        DeveloperID = developerID
                    };

                    _context.DeveloperProjects.Add(developerProject);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Reload developers if validation fails
            ViewBag.Developers = await _context.Developers
                .Include(d => d.User)
                .ToListAsync();

            return View(project);
        }

        // GET: Project/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            // Load all available developers
            ViewBag.Developers = await _context.Developers
                .Include(d => d.User)
                .ToListAsync();

            // Get developers currently assigned to this project
            project.SelectedDeveloperIDs = await _context.DeveloperProjects
                .Where(dp => dp.ProjectID == project.ProjectID)
                .Select(dp => dp.DeveloperID)
                .ToListAsync();

            return View(project);
        }

        // POST: Project/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Project project)
        {
            if (id != project.ProjectID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update the project
                    _context.Update(project);

                    await _context.SaveChangesAsync();

                    // Get the existing developer assignments
                    var existingAssignments = await _context.DeveloperProjects
                        .Where(dp => dp.ProjectID == project.ProjectID)
                        .ToListAsync();

                    // Remove the existing assignments
                    _context.DeveloperProjects.RemoveRange(existingAssignments);

                    // Add the newly selected developers
                    foreach (var developerID in project.SelectedDeveloperIDs)
                    {
                        var developerProject = new DeveloperProject
                        {
                            ProjectID = project.ProjectID,
                            DeveloperID = developerID
                        };

                        _context.DeveloperProjects.Add(developerProject);
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Projects.Any(p => p.ProjectID == project.ProjectID))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            // Reload developers if validation fails
            ViewBag.Developers = await _context.Developers
                .Include(d => d.User)
                .ToListAsync();

            return View(project);
        }

        // GET: Project/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectID == id);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Project/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project != null)
            {
                _context.Projects.Remove(project);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Project/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.ProjectUpdates)
                    .ThenInclude(pu => pu.User)
                .FirstOrDefaultAsync(p => p.ProjectID == id);

            if (project == null)
            {
                return NotFound();
            }

            // Show newest updates first
            project.ProjectUpdates = project.ProjectUpdates
                .OrderByDescending(pu => pu.CreatedDate)
                .ToList();

            // Get developers assigned to this project
            var developers = await _context.DeveloperProjects
                .Where(dp => dp.ProjectID == id)
                .Include(dp => dp.Developer)
                    .ThenInclude(d => d.User)
                .ToListAsync();

            ViewBag.Developers = developers;

            return View(project);
        }

        // GET: Project/CreateUpdate
        public async Task<IActionResult> CreateUpdate(int projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);

            if (project == null)
            {
                return NotFound();
            }

            var projectUpdate = new ProjectUpdate
            {
                ProjectID = projectId
            };

            return View(projectUpdate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUpdate(ProjectUpdate projectUpdate)
        {
            // Get the project that this update belongs to
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectID == projectUpdate.ProjectID);

            if (project == null)
            {
                return NotFound();
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Challenge();
            }
            projectUpdate.UserID = userId;
            projectUpdate.CreatedDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                // If progress was entered, update the main project progress
                if (projectUpdate.ProgressPercentage.HasValue)
                {
                    project.ProgressPercentage = projectUpdate.ProgressPercentage.Value;
                }

                // Add the project update
                _context.ProjectUpdates.Add(projectUpdate);

                // Save both the project update and project progress
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details),
                    new { id = projectUpdate.ProjectID });
            }

            return View(projectUpdate);
        }

        // GET: Project/EditUpdate/5
        public async Task<IActionResult> EditUpdate(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projectUpdate = await _context.ProjectUpdates
                .FirstOrDefaultAsync(pu => pu.UpdateID == id);

            if (projectUpdate == null)
            {
                return NotFound();
            }

            return View(projectUpdate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUpdate(ProjectUpdate projectUpdate)
        {
            // Find the existing update in the database
            var existingUpdate = await _context.ProjectUpdates
                .FirstOrDefaultAsync(pu => pu.UpdateID == projectUpdate.UpdateID);

            if (existingUpdate == null)
            {
                return NotFound();
            }

            // Check that the project exists
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectID == existingUpdate.ProjectID);

            if (project == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Update only the fields the user is allowed to edit
                existingUpdate.UpdateTitle = projectUpdate.UpdateTitle;
                existingUpdate.UpdateDescription = projectUpdate.UpdateDescription;
                existingUpdate.ProgressPercentage = projectUpdate.ProgressPercentage;

                // Update the main project progress if a progress value was entered
                if (projectUpdate.ProgressPercentage.HasValue)
                {
                    project.ProgressPercentage = projectUpdate.ProgressPercentage.Value;
                }

                // Save all changes
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details),
                    new { id = existingUpdate.ProjectID });
            }

            return View(projectUpdate);
        }

        // GET: Project/DeleteUpdate/5
        public async Task<IActionResult> DeleteUpdate(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projectUpdate = await _context.ProjectUpdates
                .FirstOrDefaultAsync(pu => pu.UpdateID == id);

            if (projectUpdate == null)
            {
                return NotFound();
            }

            return View(projectUpdate);
        }

        [HttpPost, ActionName("DeleteUpdate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUpdateConfirmed(int id)
        {
            // Find the update we want to delete
            var projectUpdate = await _context.ProjectUpdates
                .FirstOrDefaultAsync(pu => pu.UpdateID == id);

            if (projectUpdate == null)
            {
                return NotFound();
            }

            // Remember which project this update belongs to
            int projectId = projectUpdate.ProjectID;

            // Delete the update
            _context.ProjectUpdates.Remove(projectUpdate);

            // Save the deletion first
            await _context.SaveChangesAsync();

            // Find the latest remaining update for this project
            var latestUpdate = await _context.ProjectUpdates
                .Where(pu => pu.ProjectID == projectId)
                .OrderByDescending(pu => pu.CreatedDate)
                .FirstOrDefaultAsync();

            // Find the project
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectID == projectId);

            if (project != null)
            {
                if (latestUpdate != null && latestUpdate.ProgressPercentage.HasValue)
                {
                    // Use the latest update's progress
                    project.ProgressPercentage =
                        latestUpdate.ProgressPercentage.Value;
                }
                else
                {
                    // No remaining update with progress
                    project.ProgressPercentage = 0;
                }

                // Save the new project progress
                await _context.SaveChangesAsync();
            }

            // Return to the project details page
            return RedirectToAction(nameof(Details), new { id = projectId });
        }
    }
}
