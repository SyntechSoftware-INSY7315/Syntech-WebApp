namespace Syntech.WebApp.Models
{
    public class ConsultationCalendarViewModel
    {
        public int Year { get; set; }

        public int Month { get; set; }

        public string MonthName { get; set; } = string.Empty;

        public DateTime FirstDayOfMonth { get; set; }

        public DateTime FirstCalendarDay { get; set; }

        public DateTime LastCalendarDay { get; set; }

        public List<ConsultationCalendarItemViewModel> Consultations { get; set; }
            = new();
    }

    public class ConsultationCalendarItemViewModel
    {
        public int ConsultationID { get; set; }

        public DateTime BookingDate { get; set; }

        public TimeSpan BookingTime { get; set; }

        public string Status { get; set; } = string.Empty;

        public string ClientName { get; set; } = string.Empty;

        public string? Description { get; set; }
    }

}
