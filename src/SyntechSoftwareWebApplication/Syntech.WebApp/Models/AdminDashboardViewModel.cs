using System.ComponentModel.DataAnnotations;

namespace Syntech.WebApp.Models
{
    /// <summary>
    /// ViewModel used by the administrator dashboard.
    ///
    /// This class contains summary information about the users
    /// registered in the Syntech system.
    ///
    /// The dashboard uses this ViewModel rather than exposing
    /// database entities directly to the Razor view.
    /// </summary>
    public class AdminDashboardViewModel
    {
        /// <summary>
        /// Gets or sets the total number of registered users.
        /// </summary>
        [Display(Name = "Total Users")]
        public int TotalUsers { get; set; }


        /// <summary>
        /// Gets or sets the number of currently active users.
        /// </summary>
        [Display(Name = "Active Users")]
        public int ActiveUsers { get; set; }


        /// <summary>
        /// Gets or sets the number of deactivated users.
        /// </summary>
        [Display(Name = "Deactivated Users")]
        public int DeactivatedUsers { get; set; }


        /// <summary>
        /// Gets or sets the number of administrator accounts.
        /// </summary>
        [Display(Name = "Administrators")]
        public int AdminCount { get; set; }


        /// <summary>
        /// Gets or sets the number of developer accounts.
        /// </summary>
        [Display(Name = "Developers")]
        public int DeveloperCount { get; set; }


        /// <summary>
        /// Gets or sets the number of client accounts.
        /// </summary>
        [Display(Name = "Clients")]
        public int ClientCount { get; set; }


        /// <summary>
        /// Gets or sets the most recently created users.
        ///
        /// Only non-sensitive account information is included.
        /// Password hashes are never exposed to the View.
        /// </summary>
        public List<RecentUserViewModel> RecentUsers { get; set; }
            = new List<RecentUserViewModel>();
    }


    /// <summary>
    /// Represents a user displayed in the administrator dashboard's
    /// recent-users section.
    /// </summary>
    public class RecentUserViewModel
    {
        /// <summary>
        /// Gets or sets the user's database identifier.
        /// </summary>
        public int UserId { get; set; }


        /// <summary>
        /// Gets or sets the user's full name.
        /// </summary>
        public string Name { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        public string Email { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the user's application role.
        /// </summary>
        public string Role { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets whether the account is active.
        /// </summary>
        public bool IsActive { get; set; }
    }
}