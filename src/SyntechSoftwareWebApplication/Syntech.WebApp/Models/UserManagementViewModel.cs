using System.ComponentModel.DataAnnotations;

namespace Syntech.WebApp.Models
{
    /// <summary>
    /// Represents a user displayed in the administrator's
    /// user-management interface.
    ///
    /// Sensitive information such as PasswordHash is deliberately
    /// excluded from this ViewModel.
    /// </summary>
    public class UserManagementViewModel
    {
        /// <summary>
        /// Gets or sets the user's database identifier.
        /// </summary>
        [Display(Name = "User ID")]
        public int UserId { get; set; }


        /// <summary>
        /// Gets or sets the user's full name.
        /// </summary>
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the user's application role.
        /// </summary>
        [Display(Name = "Role")]
        public string Role { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets whether the user's account is currently active.
        ///
        /// Active users can authenticate.
        /// Deactivated users cannot authenticate.
        /// </summary>
        [Display(Name = "Status")]
        public bool IsActive { get; set; }
    }
}