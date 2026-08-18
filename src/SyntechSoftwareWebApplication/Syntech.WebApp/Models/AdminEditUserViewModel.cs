using System.ComponentModel.DataAnnotations;

namespace Syntech.WebApp.Models
{
    /// <summary>
    /// ViewModel used by an administrator when editing an existing user.
    ///
    /// Password information is intentionally excluded. Password changes
    /// are handled separately through the account-security functionality.
    /// </summary>
    public class AdminEditUserViewModel
    {
        /// <summary>
        /// Gets or sets the database identifier of the user.
        /// </summary>
        public int UserId { get; set; }


        /// <summary>
        /// Gets or sets the user's full name.
        /// </summary>
        [Required(ErrorMessage = "Please enter the user's full name.")]
        [StringLength(
            100,
            MinimumLength = 2,
            ErrorMessage = "The name must be between 2 and 100 characters.")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        [Required(ErrorMessage = "Please enter the user's email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(
            255,
            ErrorMessage = "The email address cannot exceed 255 characters.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the user's application role.
        ///
        /// Valid roles are Admin, Developer, and Client.
        /// </summary>
        [Required(ErrorMessage = "Please select a user role.")]
        [Display(Name = "Role")]
        public string Role { get; set; } = string.Empty;
    }
}