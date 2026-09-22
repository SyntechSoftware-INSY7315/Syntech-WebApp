using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syntech.WebApp.Data;
using Syntech.WebApp.Models;

namespace Syntech.WebApp.Controllers
{
    [Authorize]
    public class ConsultationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ConsultationController> _logger;

        public ConsultationController(
            ApplicationDbContext context,
            ILogger<ConsultationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Consultation/Manage
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Manage()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var consultations = await _context.Consultations
                .Where(c => c.ClientID == userId)
                .OrderByDescending(c => c.BookingDate)
                .ThenByDescending(c => c.BookingTime)
                .ToListAsync();

            return View("ClientBookings", consultations);
        }

        // GET: /Consultation/Create
        [Authorize(Roles = "Client")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Consultation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Create(
            DateTime bookingDate,
            TimeSpan bookingTime,
            string? description)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            // Validate description
            if (string.IsNullOrWhiteSpace(description))
            {
                ModelState.AddModelError(
                    "description",
                    "Please provide a description for your consultation.");
            }
            else if (description.Length > 1000)
            {
                ModelState.AddModelError(
                    "description",
                    "The consultation description cannot exceed 1000 characters.");
            }

            // Validate date and time
            var requestedDateTime = bookingDate.Date + bookingTime;

            if (requestedDateTime <= DateTime.Now)
            {
                ModelState.AddModelError(
                    "",
                    "The consultation date and time must be in the future.");
            }

            // Check whether the selected time is already booked
            var existingBooking = await _context.Consultations
                .AnyAsync(c =>
                    c.BookingDate == bookingDate.Date &&
                    c.BookingTime == bookingTime &&
                    c.Status != "Cancelled");

            if (existingBooking)
            {
                ModelState.AddModelError(
                    "",
                    "The selected consultation date and time is already booked. Please choose another time.");
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            var consultation = new Consultation
            {
                BookingDate = bookingDate.Date,
                BookingTime = bookingTime,
                Status = "Pending",
                ClientID = userId,
                Description = description.Trim()
            };

            _context.Consultations.Add(consultation);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Consultation {ConsultationID} created by client {ClientID}.",
                consultation.ConsultationID,
                userId);

            TempData["SuccessMessage"] =
                "Your consultation request has been submitted successfully.";

            return RedirectToAction(nameof(Manage));
        }

        // GET: /Consultation/AdminBookings
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminBookings()
        {
            var consultations = await _context.Consultations
                .Include(c => c.Client)
                .OrderBy(c => c.Status)
                .ThenBy(c => c.BookingDate)
                .ThenBy(c => c.BookingTime)
                .ToListAsync();

            return View(consultations);
        }

        // POST: /Consultation/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var consultation = await _context.Consultations
                .FirstOrDefaultAsync(c => c.ConsultationID == id);

            if (consultation == null)
            {
                return NotFound();
            }

            if (consultation.Status != "Pending")
            {
                TempData["ErrorMessage"] =
                    "Only pending consultations can be approved.";

                return RedirectToAction(nameof(AdminBookings));
            }

            consultation.Status = "Approved";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "The consultation has been approved successfully.";

            return RedirectToAction(nameof(AdminBookings));
        }

        // POST: /Consultation/Cancel
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Cancel(int id)
        {
            var consultation = await _context.Consultations
                .FirstOrDefaultAsync(c => c.ConsultationID == id);

            if (consultation == null)
            {
                return NotFound();
            }

            if (consultation.Status != "Pending" &&
                consultation.Status != "Approved")
            {
                TempData["ErrorMessage"] =
                    "This consultation cannot be cancelled.";

                return RedirectToAction(nameof(AdminBookings));
            }

            consultation.Status = "Cancelled";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "The consultation has been cancelled successfully.";

            return RedirectToAction(nameof(AdminBookings));
        }

        // POST: /Consultation/Complete
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Complete(int id)
        {
            var consultation = await _context.Consultations
                .FirstOrDefaultAsync(c => c.ConsultationID == id);

            if (consultation == null)
            {
                return NotFound();
            }

            if (consultation.Status != "Approved")
            {
                TempData["ErrorMessage"] =
                    "Only approved consultations can be marked as completed.";

                return RedirectToAction(nameof(AdminBookings));
            }

            consultation.Status = "Completed";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "The consultation has been marked as completed.";

            return RedirectToAction(nameof(AdminBookings));
        }

        // GET: /Consultation/Reschedule/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reschedule(int id)
        {
            var consultation = await _context.Consultations
                .FirstOrDefaultAsync(c => c.ConsultationID == id);

            if (consultation == null)
            {
                return NotFound();
            }

            if (consultation.Status != "Approved")
            {
                TempData["ErrorMessage"] =
                    "Only approved consultations can be rescheduled.";

                return RedirectToAction(nameof(AdminBookings));
            }

            var model = new ConsultationRescheduleViewModel
            {
                ConsultationID = consultation.ConsultationID,
                BookingDate = consultation.BookingDate,
                BookingTime = consultation.BookingTime
            };

            return View(model);
        }

        // POST: /Consultation/Reschedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reschedule(
            ConsultationRescheduleViewModel model)
        {
            var consultation = await _context.Consultations
                .FirstOrDefaultAsync(c =>
                    c.ConsultationID == model.ConsultationID);

            if (consultation == null)
            {
                return NotFound();
            }

            if (consultation.Status != "Approved")
            {
                TempData["ErrorMessage"] =
                    "Only approved consultations can be rescheduled.";

                return RedirectToAction(nameof(AdminBookings));
            }

            var requestedDateTime =
                model.BookingDate.Date + model.BookingTime;

            if (requestedDateTime <= DateTime.Now)
            {
                ModelState.AddModelError(
                    "",
                    "The consultation date and time must be in the future.");
            }

            var existingBooking = await _context.Consultations
                .AnyAsync(c =>
                    c.ConsultationID != model.ConsultationID &&
                    c.BookingDate == model.BookingDate.Date &&
                    c.BookingTime == model.BookingTime &&
                    c.Status != "Cancelled");

            if (existingBooking)
            {
                ModelState.AddModelError(
                    "",
                    "The selected consultation date and time is already booked. Please choose another time.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            consultation.BookingDate = model.BookingDate.Date;
            consultation.BookingTime = model.BookingTime;

            consultation.Status = "Approved";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "The consultation has been rescheduled successfully.";

            return RedirectToAction(nameof(AdminBookings));
        }

        // GET: /Consultation/Calendar
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Calendar(int? year, int? month)
        {
            var currentDate = DateTime.Today;

            int selectedYear = year ?? currentDate.Year;
            int selectedMonth = month ?? currentDate.Month;

            if (selectedMonth < 1 || selectedMonth > 12)
            {
                selectedMonth = currentDate.Month;
            }

            var firstDayOfMonth = new DateTime(
                selectedYear,
                selectedMonth,
                1);

            var lastDayOfMonth = firstDayOfMonth
                .AddMonths(1)
                .AddDays(-1);

            // Monday = 0, Sunday = 6
            int daysFromMonday =
                ((int)firstDayOfMonth.DayOfWeek + 6) % 7;

            var firstCalendarDay = firstDayOfMonth
                .AddDays(-daysFromMonday);

            int daysUntilSunday =
                6 - ((int)lastDayOfMonth.DayOfWeek + 6) % 7;

            var lastCalendarDay = lastDayOfMonth
                .AddDays(daysUntilSunday);

            var consultations = await _context.Consultations
                .Include(c => c.Client)
                .Where(c =>
                    c.BookingDate >= firstCalendarDay &&
                    c.BookingDate <= lastCalendarDay)
                .OrderBy(c => c.BookingDate)
                .ThenBy(c => c.BookingTime)
                .ToListAsync();

            var model = new ConsultationCalendarViewModel
            {
                Year = selectedYear,
                Month = selectedMonth,
                MonthName = firstDayOfMonth.ToString("MMMM yyyy"),
                FirstDayOfMonth = firstDayOfMonth,
                FirstCalendarDay = firstCalendarDay,
                LastCalendarDay = lastCalendarDay,

                Consultations = consultations
                    .Select(c => new ConsultationCalendarItemViewModel
                    {
                        ConsultationID = c.ConsultationID,
                        BookingDate = c.BookingDate,
                        BookingTime = c.BookingTime,
                        Status = c.Status,
                        ClientName = c.Client?.Name ?? "Unknown client",
                        Description = c.Description
                    })
                    .ToList()
            };

            return View(model);
        }

    }
}
