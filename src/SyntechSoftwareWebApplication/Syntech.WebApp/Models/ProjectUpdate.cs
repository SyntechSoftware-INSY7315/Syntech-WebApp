using System.ComponentModel.DataAnnotations;

namespace Syntech.WebApp.Models
{
    public class ProjectUpdate
    {
        [Key]
        public int UpdateID { get; set; }
        public int ProjectID { get; set; }
        public int UserID { get; set; }

        [Required(ErrorMessage = "Please enter an update title.")]
        [StringLength(200)]
        public string UpdateTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter an update description.")]
        public string UpdateDescription { get; set; } = string.Empty;

        [Range(0, 100)]
        public decimal? ProgressPercentage { get; set; }

        public DateTime CreatedDate { get; set; }

        public Project? Project { get; set; }
        public User? User { get; set; }   // Chris’s User
    }
}
