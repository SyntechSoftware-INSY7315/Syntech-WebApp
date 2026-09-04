using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syntech.WebApp.Models.Ticket;

namespace Syntech.WebApp.Models.Ticket
{
    public class CreateTicketViewModel
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Priority { get; set; } = "Medium";
    }

    public class TicketDetailViewModel
    {
        public SupportTicket Ticket { get; set; } = null!;
        public List<TicketReply> Replies { get; set; } = new();
        public List<SelectListItem> Developers { get; set; } = new();
    }

    public class TicketListViewModel
    {
        public List<SupportTicket> Tickets { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
        public string? PriorityFilter { get; set; }
        public int OpenCount { get; set; }
        public int InProgressCount { get; set; }
        public int ResolvedCount { get; set; }
        public int ClosedCount { get; set; }
    }
}
