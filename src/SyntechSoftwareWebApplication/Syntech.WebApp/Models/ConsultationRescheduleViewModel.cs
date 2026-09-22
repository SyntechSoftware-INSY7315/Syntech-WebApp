using System.ComponentModel.DataAnnotations;

namespace Syntech.WebApp.Models
{
    public class ConsultationRescheduleViewModel
    {
        public int ConsultationID { get; set; }

        [Required(ErrorMessage = "Please select a consultation date.")]
        public DateTime BookingDate { get; set; }

        [Required(ErrorMessage = "Please select a consultation time.")]
        public TimeSpan BookingTime { get; set; }
    }
}
