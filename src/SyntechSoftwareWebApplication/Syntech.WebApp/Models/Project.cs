using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Syntech.WebApp.Models
{
    public class Project
    {
        [Key]
        public int ProjectID { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal ProgressPercentage { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? Deadline { get; set; }
        public int ClientID { get; set; }
        public int AdminID { get; set; }

        public List<ProjectUpdate> ProjectUpdates { get; set; } = new();

        [NotMapped]
        public List<int> SelectedDeveloperIDs { get; set; } = new();
    }
}
