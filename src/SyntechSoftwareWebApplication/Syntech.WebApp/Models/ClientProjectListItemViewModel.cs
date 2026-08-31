namespace Syntech.WebApp.Models
{
    public class ClientProjectListItemViewModel
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal ProgressPercentage { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? Deadline { get; set; }
    }
}
