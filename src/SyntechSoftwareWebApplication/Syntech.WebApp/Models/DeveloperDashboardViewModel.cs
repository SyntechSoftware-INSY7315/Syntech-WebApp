namespace Syntech.WebApp.Models
{
    /// <summary>
    /// ViewModel used by the Developer Dashboard.
    ///
    /// This ViewModel contains only information that the developer
    /// dashboard needs to display. Database entities are not passed
    /// directly to the Razor view.
    /// </summary>
    public class DeveloperDashboardViewModel
    {
        /// <summary>
        /// Gets or sets the authenticated developer's database ID.
        /// </summary>
        public int UserId { get; set; }


        /// <summary>
        /// Gets or sets the authenticated developer's full name.
        /// </summary>
        public string Name { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the authenticated developer's email address.
        /// </summary>
        public string Email { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the authenticated user's role.
        ///
        /// For this dashboard this value should be "Developer".
        /// </summary>
        public string Role { get; set; } = string.Empty;
    }
}