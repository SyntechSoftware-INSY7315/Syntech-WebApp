using System.ComponentModel.DataAnnotations;

namespace Syntech.WebApp.Models
{
    /// <summary>
    /// Represents the data displayed and edited on the user's profile page.
    ///
    /// This ViewModel intentionally contains only information that the user
    /// is allowed to view or edit through the profile interface.
    ///
    /// IMPORTANT:
    /// The user's password is never included in this ViewModel.
    ///
    /// The user's role is also not editable through this ViewModel.
    /// Role changes are an administrative operation and will be handled
    /// separately through administrator user management.
    /// </summary>
    public class ProfileViewModel
    {
        /// <summary>
        /// Gets or sets the authenticated user's database identifier.
        ///
        /// This value is displayed as read-only information and is NOT
        /// accepted from the browser as the source of truth when loading
        /// or updating the profile.
        ///
        /// The actual user ID is always obtained from the authenticated
        /// user's NameIdentifier claim by the controller.
        /// </summary>
        [Display(Name = "User ID")]
        public int UserId { get; set; }


        /// <summary>
        /// Gets or sets the user's full name.
        ///
        /// This is the only user information that will initially be editable
        /// through the profile page.
        /// </summary>
        [Required(ErrorMessage = "Please enter your name.")]
        [StringLength(
            100,
            MinimumLength = 2,
            ErrorMessage =
                "Your name must be between 2 and 100 characters.")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the user's email address.
        ///
        /// Email is displayed as read-only information for now.
        ///
        /// We are deliberately not allowing users to change their email
        /// through the initial profile implementation because changing an
        /// email address affects authentication and the database's unique
        /// email constraint.
        ///
        /// A dedicated email-change workflow can be implemented later if
        /// required by the project.
        /// </summary>
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the user's application role.
        ///
        /// The role is displayed for information only.
        ///
        /// Users must never be allowed to submit their own role through
        /// the profile form because doing so could allow a Client to attempt
        /// to elevate themselves to Developer or Administrator.
        /// </summary>
        [Display(Name = "Role")]
        public string Role { get; set; } = string.Empty;
    }
}