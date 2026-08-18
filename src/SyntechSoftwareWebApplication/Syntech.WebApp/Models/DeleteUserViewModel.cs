namespace Syntech.WebApp.Models
{
    /// <summary>
    /// ViewModel used to display the confirmation page before an
    /// administrator permanently deletes a user.
    ///
    /// Password information is intentionally excluded.
    /// </summary>
    public class DeleteUserViewModel
    {
        /// <summary>
        /// Gets or sets the database identifier of the user.
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
    }
}