namespace Syntech.WebApp.Models
{
    /// <summary>
    /// Lightweight list item for projects shown on the Developer Dashboard.
    /// </summary>
    public class DeveloperProjectListItemViewModel
    {
        public int ProjectID { get; set; }

        public string ProjectName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public decimal ProgressPercentage { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? Deadline { get; set; }
    }
}