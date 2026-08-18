using System.ComponentModel.DataAnnotations;

namespace Syntech.WebApp.Models
{
    /// <summary>
    /// ViewModel used by an administrator when creating a new user account.
    ///
    /// This ViewModel is intentionally separate from the database User
    /// entity.
    ///
    /// The administrator can provide:
    ///
    /// - User's name
    /// - User's email address
    /// - Initial password
    /// - Password confirmation
    /// - Application role
    ///
    /// The PasswordHash database property is never exposed to the view.
    /// The password is hashed by AuthenticationService before it is
    /// stored in SQL Server.
    /// </summary>
    public class AdminCreateUserViewModel
    {
        /// <summary>
        /// Gets or sets the full name of the new user.
        /// </summary>
        [Required(
            ErrorMessage =
                "Please enter the user's full name.")]
        [StringLength(
            100,
            MinimumLength = 2,
            ErrorMessage =
                "The user's name must be between 2 and 100 characters.")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the email address of the new user.
        ///
        /// The email address is used as the user's login identifier and
        /// must be unique in the database.
        /// </summary>
        [Required(
            ErrorMessage =
                "Please enter the user's email address.")]
        [EmailAddress(
            ErrorMessage =
                "Please enter a valid email address.")]
        [StringLength(
            255,
            ErrorMessage =
                "The email address cannot exceed 255 characters.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the initial password assigned to the new user.
        ///
        /// The password is held only while the request is being processed.
        /// It is never stored directly in the database.
        /// </summary>
        [Required(
            ErrorMessage =
                "Please enter an initial password.")]
        [DataType(DataType.Password)]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage =
                "Password must be between 8 and 100 characters.")]
        [Display(Name = "Initial Password")]
        public string Password { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the confirmation of the initial password.
        /// </summary>
        [Required(
            ErrorMessage =
                "Please confirm the initial password.")]
        [DataType(DataType.Password)]
        [Compare(
            nameof(Password),
            ErrorMessage =
                "The passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the role assigned to the new user.
        ///
        /// The server will independently validate this value.
        ///
        /// Valid roles are:
        ///
        /// - Admin
        /// - Developer
        /// - Client
        ///
        /// A hidden field, select list, or other browser-side control
        /// must never be considered sufficient authorization for choosing
        /// a role. The controller and service validate the value again
        /// on the server.
        /// </summary>
        [Required(
            ErrorMessage =
                "Please select a user role.")]
        [Display(Name = "Role")]
        public string Role { get; set; } = string.Empty;
    }
}