using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Syntech.WebApp.Models;

namespace Syntech.WebApp.Models.Ticket
{
    [Table("TicketReply")]
    public class TicketReply
    {
        [Key]
        [Column("replyID")]
        public int ReplyId { get; set; }

        [Column("ticketID")]
        public int TicketId { get; set; }

        [ForeignKey("TicketId")]
        public SupportTicket? SupportTicket { get; set; }

        [Column("userID")]
        public int AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public User? Author { get; set; }

        [Required]
        [Column("replyMessage")]
        public string Message { get; set; } = string.Empty;

        [Column("repliedDate")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
