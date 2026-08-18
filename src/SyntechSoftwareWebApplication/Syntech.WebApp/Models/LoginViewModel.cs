using System.ComponentModel.DataAnnotations;

namespace Syntech.WebApp.Models
{
    /// <summary>
    /// View model used by the login form.
    ///
    /// This model contains only the information required to authenticate
    /// an existing user.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Gets or sets the email address used to identify the account.
        /// </summary>
        [Required(
        ErrorMessage =
        "Please enter your email address.")]
        [EmailAddress(
        ErrorMessage =
        "Please enter a valid email address.")]
        [StringLength(
        255,
        ErrorMessage =
        "Your email address cannot exceed 255 characters.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's plain-text password.
        ///
        /// The password is never stored in the database. It is passed to
        /// AuthenticationService, which verifies it against the stored hash.
        /// </summary>
        [Required(
            ErrorMessage =
                "Please enter your password.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets whether the user wants the authentication session
        /// to persist beyond the current browser session.
        ///
        /// This will be implemented carefully when the login functionality
        /// is added.
        /// </summary>
        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

}