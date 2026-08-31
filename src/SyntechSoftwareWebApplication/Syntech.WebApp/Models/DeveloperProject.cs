namespace Syntech.WebApp.Models
{
    public class DeveloperProject
    {
        public int DeveloperID { get; set; }
        public int ProjectID { get; set; }

        public Project? Project { get; set; }
        public Developer? Developer { get; set; }  // Chris’s Developer (UserId)
    }
}
