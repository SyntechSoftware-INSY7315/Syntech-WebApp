using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syntech.WebApp.Data;
using Syntech.WebApp.Models;

namespace Syntech.WebApp.Controllers
{
    [Authorize]
    public class DocumentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<DocumentController> _logger;

        private static readonly string[] AllowedExtensions =
        {
            ".pdf",
            ".docx",
            ".doc",
            ".xlsx",
            ".png",
            ".jpg",
            ".zip"
        };

        public DocumentController(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            ILogger<DocumentController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        /* 
         =====================================================================
         DOCUMENT MANAGEMENT LANDING PAGE
         =====================================================================
         Displays the role-specific document management page.
         
         Client     -> ClientDocuments.cshtml
         Developer  -> DeveloperDocuments.cshtml
         Admin      -> AdminDocuments.cshtml
        */

        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            if (!TryGetAuthenticatedUserId(out int userId))
            {
                return Challenge();
            }

            string role =
                User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            // -------------------------------------------------------------
            // CLIENT
            // -------------------------------------------------------------

            if (role == "Client")
            {
                List<Project> projects = await _context.Projects
                    .AsNoTracking()
                    .Where(p => p.ClientID == userId)
                    .OrderBy(p => p.ProjectName)
                    .ToListAsync();

                return View("ClientDocuments", projects);
            }

            // -------------------------------------------------------------
            // DEVELOPER
            // -------------------------------------------------------------

            if (role == "Developer")
            {
                List<Project> projects = await _context.DeveloperProjects
                    .AsNoTracking()
                    .Where(dp => dp.DeveloperID == userId)
                    .Include(dp => dp.Project)
                    .Where(dp => dp.Project != null)
                    .Select(dp => dp.Project!)
                    .OrderBy(p => p.ProjectName)
                    .ToListAsync();

                return View("DeveloperDocuments", projects);
            }

            // -------------------------------------------------------------
            // ADMIN
            // -------------------------------------------------------------

            if (role == "Admin")
            {
                List<Project> projects = await _context.Projects
                    .AsNoTracking()
                    .OrderBy(p => p.ProjectName)
                    .ToListAsync();

                return View("AdminDocuments", projects);
            }

            // -------------------------------------------------------------
            // UNKNOWN / UNSUPPORTED ROLE
            // -------------------------------------------------------------

            return Forbid();
        }

        [HttpGet]
        public async Task<IActionResult> Index(int projectId)
        {
            if (!TryGetAuthenticatedUserId(out int userId))
            {
                return Challenge();
            }

            if (!await CanAccessProjectAsync(projectId, userId))
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

            List<Document> documents = await _context.Documents
                .AsNoTracking()
                .Include(d => d.Uploader)
                .Where(d => d.ProjectID == projectId)
                .OrderByDescending(d => d.UploadDate)
                .ToListAsync();

            ViewBag.ProjectName = project.ProjectName;
            ViewBag.ProjectID = projectId;

            return View(documents);
        }


        // =====================================================================
        // UPLOAD - FORM
        // =====================================================================

        /// <summary>
        /// Displays the document upload form.
        ///
        /// GET:
        /// /Document/Upload?projectId=5
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Upload(int projectId)
        {
            if (!TryGetAuthenticatedUserId(out int userId))
            {
                return Unauthorized();
            }

            if (!await CanAccessProjectAsync(projectId, userId))
            {
                return Forbid();
            }

            string role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            ViewBag.ProjectID = projectId;
            ViewBag.AllowedCategories = GetAllowedCategories(role);

            return View();
        }


        // =====================================================================
        // UPLOAD - PROCESS
        // =====================================================================

        /// <summary>
        /// Processes a document upload.
        ///
        /// The authenticated user's ID and file storage information are
        /// determined server-side.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(
            DocumentUploadViewModel model)
        {
            if (!TryGetAuthenticatedUserId(out int userId))
            {
                return Unauthorized();
            }

            if (!await CanAccessProjectAsync(model.ProjectID, userId))
            {
                return Forbid();
            }

            string role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            string category = model.Category?.Trim() ?? string.Empty;

            if (!IsCategoryAllowed(role, category))
            {
                ModelState.AddModelError(
                    nameof(model.Category),
                    "You are not allowed to upload documents in this category.");
            }

            Project? project = await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    p => p.ProjectID == model.ProjectID);

            if (project == null)
            {
                return NotFound();
            }

            ViewBag.ProjectName = project.ProjectName;
            ViewBag.ProjectID = model.ProjectID;
            ViewBag.AllowedCategories = GetAllowedCategories(role);

            // -------------------------------------------------------------
            // Validate the submitted ViewModel.
            // -------------------------------------------------------------

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // -------------------------------------------------------------
            // Validate that a file was actually supplied.
            // -------------------------------------------------------------

            if (model.File == null || model.File.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(model.File),
                    "Please select a file to upload.");

                return View(model);
            }

            // -------------------------------------------------------------
            // Get the extension from the uploaded filename.
            //
            // Path.GetExtension is used rather than trusting a MIME type
            // supplied by the browser.
            // -------------------------------------------------------------

            string extension =
                Path.GetExtension(model.File.FileName)
                    .ToLowerInvariant();

            // -------------------------------------------------------------
            // Check whether the extension is supported.
            // -------------------------------------------------------------

            if (!AllowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    nameof(model.File),
                    "This file type is not supported.");

                return View(model);
            }

            // -------------------------------------------------------------
            // Generate a unique server-side filename.
            //
            // We never use the original filename as the physical filename.
            // -------------------------------------------------------------

            string generatedFileName =
                $"{Guid.NewGuid():N}{extension}";

            // -------------------------------------------------------------
            // Store uploaded files outside wwwroot.
            //
            // This prevents users from accessing uploaded files directly
            // through a predictable public URL.
            // -------------------------------------------------------------

            string uploadsFolder =
                Path.Combine(
                    _environment.ContentRootPath,
                    "App_Data",
                    "Documents");

            Directory.CreateDirectory(uploadsFolder);

            string physicalPath =
                Path.Combine(
                    uploadsFolder,
                    generatedFileName);

            // -------------------------------------------------------------
            // Save the physical file.
            // -------------------------------------------------------------

            await using (
                FileStream stream =
                    new FileStream(
                        physicalPath,
                        FileMode.CreateNew))
            {
                await model.File.CopyToAsync(stream);
            }

            // -------------------------------------------------------------
            // Create the database metadata record.
            // -------------------------------------------------------------

            Document document = new Document
            {
                FileName = generatedFileName,

                OriginalFileName =
                    Path.GetFileName(model.File.FileName),

                FileExtension = extension,

                FileType = model.File.ContentType,

                FilePath = Path.Combine(
                    "App_Data",
                    "Documents",
                    generatedFileName),

                FileSize = model.File.Length,

                UploadedBy = userId,

                UploadDate = DateTime.Now,

                ProjectID = model.ProjectID,

                Category = model.Category.Trim()
            };

            // -------------------------------------------------------------
            // Save document metadata to the existing database.
            // -------------------------------------------------------------

            _context.Documents.Add(document);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                // If the database insert fails after the physical file
                // was created, remove the physical file so that an orphaned
                // upload is not left behind.
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }

                throw;
            }

            _logger.LogInformation(
                "User {UserId} uploaded document {DocumentId} " +
                "to project {ProjectId}.",
                userId,
                document.DocumentID,
                document.ProjectID);

            TempData["Success"] =
                "The document was uploaded successfully.";

            return RedirectToAction(
                nameof(Index),
                new
                {
                    projectId = model.ProjectID
                });
        }

        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            if (!TryGetAuthenticatedUserId(out int userId))
            {
                return Challenge();
            }

            Document? document = await _context.Documents
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DocumentID == id);

            if (document == null)
            {
                return NotFound();
            }

            if (!await CanAccessProjectAsync(document.ProjectID, userId))
            {
                return Forbid();
            }

            string uploadsFolder = Path.Combine(
                _environment.ContentRootPath,
                "App_Data",
                "Documents");

            string safeFileName = Path.GetFileName(document.FileName);

            string physicalPath = Path.Combine(
                uploadsFolder,
                safeFileName);

            if (!System.IO.File.Exists(physicalPath))
            {
                _logger.LogWarning(
                    "Document file is missing. Document ID: {DocumentId}",
                    document.DocumentID);

                return NotFound("The document file could not be found.");
            }

            string contentType =
                document.FileType ?? "application/octet-stream";

            return PhysicalFile(
                physicalPath,
                contentType,
                document.OriginalFileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!TryGetAuthenticatedUserId(out int userId))
            {
                return Challenge();
            }

            Document? document = await _context.Documents
                .FirstOrDefaultAsync(d => d.DocumentID == id);

            if (document == null)
            {
                return NotFound();
            }

            string role =
                User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            if (!await CanAccessProjectAsync(document.ProjectID, userId))
            {
                return Forbid();
            }

            if (role == "Client")
            {
                return Forbid();
            }

            if (role == "Developer" &&
                document.UploadedBy != userId)
            {
                return Forbid();
            }

            if (role != "Admin" && role != "Developer")
            {
                return Forbid();
            }

            string uploadsFolder = Path.Combine(
                _environment.ContentRootPath,
                "App_Data",
                "Documents");

            string safeFileName = Path.GetFileName(document.FileName);

            string physicalPath = Path.Combine(
                uploadsFolder,
                safeFileName);

            try
            {
                // Delete the physical file first.
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }

                // Then delete the database record.
                _context.Documents.Remove(document);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting document {DocumentId}.",
                    document.DocumentID);

                TempData["Error"] =
                    "The document could not be deleted.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        projectId = document.ProjectID
                    });
            }

            _logger.LogInformation(
                "User {UserId} deleted document {DocumentId}.",
                userId,
                document.DocumentID);

            TempData["Success"] =
                "The document was deleted successfully.";

            return RedirectToAction(
                nameof(Index),
                new
                {
                    projectId = document.ProjectID
                });
        }


        // =====================================================================
        // PROJECT AUTHORIZATION
        // =====================================================================

        /// <summary>
        /// Determines whether the authenticated user may access a project.
        ///
        /// Admin:
        ///     May access any project.
        ///
        /// Client:
        ///     May access projects where Project.ClientID matches their
        ///     authenticated UserId.
        ///
        /// Developer:
        ///     May access projects assigned through DeveloperProject.
        /// </summary>
        private async Task<bool> CanAccessProjectAsync(
            int projectId,
            int userId)
        {
            string role =
                User.FindFirstValue(
                    ClaimTypes.Role) ?? string.Empty;

            // -------------------------------------------------------------
            // Administrators can access every project.
            // -------------------------------------------------------------

            if (role == "Admin")
            {
                return await _context.Projects
                    .AnyAsync(p =>
                        p.ProjectID == projectId);
            }

            // -------------------------------------------------------------
            // Clients can access their own projects.
            // -------------------------------------------------------------

            if (role == "Client")
            {
                return await _context.Projects
                    .AnyAsync(p =>
                        p.ProjectID == projectId &&
                        p.ClientID == userId);
            }

            // -------------------------------------------------------------
            // Developers can access projects assigned to them.
            // DeveloperProject.DeveloperID contains the Developer.UserId.
            // -------------------------------------------------------------

            if (role == "Developer")
            {
                return await _context.DeveloperProjects
                    .AnyAsync(dp =>
                        dp.ProjectID == projectId &&
                        dp.DeveloperID == userId);
            }

            return false;
        }


        // =====================================================================
        // AUTHENTICATED USER ID
        // =====================================================================

        /// <summary>
        /// Retrieves the authenticated user's database ID from the
        /// NameIdentifier authentication claim.
        /// </summary>
        private bool TryGetAuthenticatedUserId(
            out int userId)
        {
            string? userIdClaim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            return int.TryParse(
                userIdClaim,
                out userId);
        }

        private List<string> GetAllowedCategories(string role)
        {
            return role switch
            {
                "Admin" => new List<string>
        {
            "Requirements",
            "Contracts",
            "Deliverables",
            "Reports",
            "Other"
        },

                "Developer" => new List<string>
        {
            "Deliverables",
            "Reports",
            "Other"
        },

                "Client" => new List<string>
        {
            "Requirements",
            "Other"
        },

                _ => new List<string>()
            };
        }

        private bool IsCategoryAllowed(string role, string category)
        {
            return GetAllowedCategories(role)
                .Contains(category, StringComparer.OrdinalIgnoreCase);
        }

    }
}
