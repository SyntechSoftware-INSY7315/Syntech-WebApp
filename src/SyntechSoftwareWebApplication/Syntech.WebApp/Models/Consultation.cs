using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Syntech.WebApp.Models
{
    [Table("Consultation")]
    public class Consultation
    {
        [Key]
        [Column("consultationID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ConsultationID { get; set; }

        [Required]
        [Column("bookingDate")]
        public DateTime BookingDate { get; set; }

        [Required]
        [Column("bookingTime")]
        public TimeSpan BookingTime { get; set; }

        [Required]
        [StringLength(30)]
        [Column("status")]
        public string Status { get; set; } = "Pending";

        [Required]
        [Column("clientID")]
        public int ClientID { get; set; }

        [Required]
        [StringLength(1000)]
        [Column("description")]
        public string? Description { get; set; }

        [ForeignKey(nameof(ClientID))]
        public User? Client { get; set; }
    }
}
