using System.ComponentModel.DataAnnotations;

namespace Syntech.WebApp.Models
{
    /// <summary>
    /// View model used by the user registration form.
    ///
    /// A view model is intentionally separate from the database User entity.
    /// This prevents the registration form from directly controlling fields
    /// that should only be managed by the application.
    ///
    /// For example, Password is required during registration but is never
    /// stored directly in the database. It is converted into a secure hash
    /// by AuthenticationService.
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// Gets or sets the user's full name.
        /// </summary>
        [Required(
            ErrorMessage = "Please enter your full name.")]
        [StringLength(
            100,
            ErrorMessage =
                "Your name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the user's email address.
        ///
        /// The email address will also be used as the user's login identifier.
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
        /// Gets or sets the password entered by the user.
        ///
        /// This is only held in memory while processing the registration
        /// request. It must never be stored directly in the database.
        /// </summary>
        [Required(
            ErrorMessage =
                "Please enter a password.")]
        [DataType(DataType.Password)]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage =
                "Password must be between 8 and 100 characters.")]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the confirmation of the user's password.
        ///
        /// This ensures the user did not accidentally mistype their password
        /// during registration.
        /// </summary>
        [Required(
            ErrorMessage =
                "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare(
            nameof(Password),
            ErrorMessage =
                "The passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the role requested during registration.
        ///
        /// IMPORTANT:
        /// The public registration process will only allow the roles that
        /// the application's registration policy permits.
        ///
        /// Administrator accounts must NOT be created by allowing a public
        /// user to simply select "Admin" from a form.
        /// </summary>
        [Required(
            ErrorMessage =
                "Please select an account type.")]
        [Display(Name = "Account Type")]
        public string Role { get; set; } = string.Empty;
    }
}
