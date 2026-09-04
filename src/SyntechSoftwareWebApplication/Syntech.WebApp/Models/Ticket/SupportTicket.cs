using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Syntech.WebApp.Models;

namespace Syntech.WebApp.Models.Ticket
{
    [Table("SupportTicket")]
    public class SupportTicket
    {
        [Key]
        [Column("ticketID")]
        public int TicketId { get; set; }

        [Required]
        [StringLength(150)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Column("status")]
        public string Status { get; set; } = "Open";

        [Required]
        [StringLength(10)]
        [Column("priority")]
        public string Priority { get; set; } = "Medium";

        [Column("createdDate")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("clientID")]
        public int ClientId { get; set; }

        [ForeignKey("ClientId")]
        public User? Client { get; set; }

        [Column("developerID")]
        public int? AssignedDeveloperId { get; set; }

        [ForeignKey("AssignedDeveloperId")]
        public User? AssignedDeveloper { get; set; }

        public ICollection<TicketReply> Replies { get; set; } = new List<TicketReply>();
    }
}
