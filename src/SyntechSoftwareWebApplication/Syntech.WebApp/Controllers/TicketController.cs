using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Syntech.WebApp.Data;
using Syntech.WebApp.Models.Ticket;
using System.Security.Claims;

namespace Syntech.WebApp.Controllers
{
    [Authorize]
    public class TicketController : Controller
    {
        private readonly ApplicationDbContext _db;

        public TicketController(ApplicationDbContext db)
        {
            _db = db;
        }

        private int GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : 0;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        }

        public async Task<IActionResult> Index(
            string? search,
            string? status,
            string? priority)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();

            var query = _db.SupportTickets
                .Include(t => t.Client)
                .Include(t => t.AssignedDeveloper)
                .AsQueryable();

            if (role == "Client")
                query = query.Where(t => t.ClientId == userId);
            else if (role == "Developer")
                query = query.Where(t => t.AssignedDeveloperId == userId);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(t =>
                    t.Title.Contains(search) ||
                    (t.Client != null && t.Client.Name.Contains(search)));

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(t => t.Status == status);

            if (!string.IsNullOrWhiteSpace(priority))
                query = query.Where(t => t.Priority == priority);

            var tickets = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var vm = new TicketListViewModel
            {
                Tickets = tickets,
                SearchTerm = search,
                StatusFilter = status,
                PriorityFilter = priority,
                OpenCount = tickets.Count(t => t.Status == "Open"),
                InProgressCount = tickets.Count(t => t.Status == "InProgress"),
                ResolvedCount = tickets.Count(t => t.Status == "Resolved"),
                ClosedCount = tickets.Count(t => t.Status == "Closed")
            };

            return View(vm);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();

            var ticket = await _db.SupportTickets
                .Include(t => t.Client)
                .Include(t => t.AssignedDeveloper)
                .Include(t => t.Replies)
                    .ThenInclude(r => r.Author)
                .FirstOrDefaultAsync(t => t.TicketId == id);

            if (ticket == null)
                return NotFound();

            if (role == "Client" && ticket.ClientId != userId)
                return Forbid();

            if (role == "Developer" && ticket.AssignedDeveloperId != userId)
                return Forbid();

            var developers = await _db.Users
                .Where(u => u.Role == "Developer" && u.IsActive)
                .Select(u => new SelectListItem
                {
                    Value = u.UserId.ToString(),
                    Text = u.Name
                })
                .ToListAsync();

            var vm = new TicketDetailViewModel
            {
                Ticket = ticket,
                Replies = ticket.Replies.OrderBy(r => r.CreatedAt).ToList(),
                Developers = developers
            };

            return View(vm);
        }

        [Authorize(Roles = "Client")]
        public IActionResult Create()
        {
            return View(new CreateTicketViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Create(CreateTicketViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var ticket = new SupportTicket
            {
                Title = vm.Title.Trim(),
                Description = vm.Description.Trim(),
                Priority = vm.Priority,
                ClientId = GetCurrentUserId(),
                Status = "Open",
                CreatedAt = DateTime.UtcNow
            };

            _db.SupportTickets.Add(ticket);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Your support ticket has been submitted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int ticketId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Reply message cannot be empty.";
                return RedirectToAction(nameof(Detail), new { id = ticketId });
            }

            var ticket = await _db.SupportTickets.FindAsync(ticketId);
            if (ticket == null)
                return NotFound();

            var role = GetCurrentUserRole();
            var userId = GetCurrentUserId();

            if (role == "Client" && ticket.ClientId != userId)
                return Forbid();

            if (role == "Developer" && ticket.AssignedDeveloperId != userId)
                return Forbid();

            var reply = new TicketReply
            {
                TicketId = ticketId,
                AuthorId = userId,
                Message = message.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            

            _db.TicketReplies.Add(reply);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Reply sent.";
            return RedirectToAction(nameof(Detail), new { id = ticketId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Developer")]
        public async Task<IActionResult> UpdateStatus(int ticketId, string status)
        {
            var validStatuses = new[] { "Open", "InProgress", "Resolved", "Closed" };
            if (!validStatuses.Contains(status))
            {
                TempData["Error"] = "Invalid status.";
                return RedirectToAction(nameof(Detail), new { id = ticketId });
            }

            var ticket = await _db.SupportTickets.FindAsync(ticketId);
            if (ticket == null)
                return NotFound();

            ticket.Status = status;
            
            await _db.SaveChangesAsync();

            TempData["Success"] = "Ticket status updated.";
            return RedirectToAction(nameof(Detail), new { id = ticketId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Assign(int ticketId, int developerId)
        {
            var ticket = await _db.SupportTickets.FindAsync(ticketId);
            if (ticket == null)
                return NotFound();

            var developer = await _db.Users
                .FirstOrDefaultAsync(u => u.UserId == developerId && u.Role == "Developer");

            if (developer == null)
            {
                TempData["Error"] = "Invalid developer selected.";
                return RedirectToAction(nameof(Detail), new { id = ticketId });
            }

            ticket.AssignedDeveloperId = developerId;
            ticket.Status = "InProgress";
            
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Ticket assigned to {developer.Name}.";
            return RedirectToAction(nameof(Detail), new { id = ticketId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePriority(int ticketId, string priority)
        {
            var validPriorities = new[] { "Low", "Medium", "High" };
            if (!validPriorities.Contains(priority))
            {
                TempData["Error"] = "Invalid priority.";
                return RedirectToAction(nameof(Detail), new { id = ticketId });
            }

            var ticket = await _db.SupportTickets.FindAsync(ticketId);
            if (ticket == null)
                return NotFound();

            ticket.Priority = priority;
            
            await _db.SaveChangesAsync();

            TempData["Success"] = "Priority updated.";
            return RedirectToAction(nameof(Detail), new { id = ticketId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var ticket = await _db.SupportTickets
                .Include(t => t.Replies)
                .FirstOrDefaultAsync(t => t.TicketId == id);

            if (ticket == null)
                return NotFound();

            _db.SupportTickets.Remove(ticket);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Ticket deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
