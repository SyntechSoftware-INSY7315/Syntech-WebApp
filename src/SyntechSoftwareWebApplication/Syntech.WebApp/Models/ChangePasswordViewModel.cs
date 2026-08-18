using System.ComponentModel.DataAnnotations;

namespace Syntech.WebApp.Models
{
    /// <summary>
    /// View model used when an authenticated user changes their password.
    ///
    /// The model contains the current password, the requested new password,
    /// and a confirmation of the new password.
    ///
    /// IMPORTANT:
    ///
    /// Password values exist only while processing the request.
    /// They are never stored directly in the database.
    ///
    /// The AuthenticationService is responsible for verifying the current
    /// password and hashing the new password before it is stored.
    /// </summary>
    public class ChangePasswordViewModel
    {
        /// <summary>
        /// Gets or sets the user's existing password.
        ///
        /// This password is verified against the password hash currently
        /// stored in dbo.User.
        /// </summary>
        [Required(
            ErrorMessage =
                "Please enter your current password.")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the new password requested by the user.
        ///
        /// The same baseline password policy used during registration is
        /// applied here:
        ///
        /// - Minimum 8 characters
        /// - Maximum 100 characters
        /// </summary>
        [Required(
            ErrorMessage =
                "Please enter a new password.")]
        [DataType(DataType.Password)]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage =
                "Password must be between 8 and 100 characters.")]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the confirmation of the new password.
        ///
        /// The value must exactly match NewPassword.
        /// </summary>
        [Required(
            ErrorMessage =
                "Please confirm your new password.")]
        [DataType(DataType.Password)]
        [Compare(
            nameof(NewPassword),
            ErrorMessage =
                "The new passwords do not match.")]
        [Display(Name = "Confirm New Password")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}