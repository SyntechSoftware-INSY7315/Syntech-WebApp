using Syntech.WebApp.Models;

namespace Syntech.WebApp.Services
{
    /// <summary>
    /// Defines authentication and account-management operations
    /// used by the Syntech application.
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Authenticates a user using their email address and password.
        /// </summary>
        Task<User?> AuthenticateAsync(
            string email,
            string password);

        /// <summary>
        /// Checks whether an email address is already registered.
        /// </summary>
        Task<bool> EmailExistsAsync(
            string email);

        /// <summary>
        /// Creates an account through public registration.
        /// </summary>
        Task<User> RegisterAsync(
            string name,
            string email,
            string password,
            string role);

        /// <summary>
        /// Changes an authenticated user's password.
        /// </summary>
        Task<bool> ChangePasswordAsync(
            int userId,
            string currentPassword,
            string newPassword);

        /// <summary>
        /// Creates a new user on behalf of an administrator.
        /// </summary>
        Task<User> CreateUserByAdminAsync(
            string name,
            string email,
            string password,
            string role);

        /// <summary>
        /// Updates an existing user's name, email address, and role.
        /// </summary>
        Task<User> UpdateUserByAdminAsync(
            int userId,
            string name,
            string email,
            string role);

        /// <summary>
        /// Changes whether an existing user account is active.
        /// </summary>
        Task<User> SetUserActiveStatusAsync(
            int userId,
            bool isActive);

        /// <summary>
        /// Deletes an existing user account.
        ///
        /// The service prevents an administrator from deleting their
        /// own account.
        /// </summary>
        Task DeleteUserByAdminAsync(
            int userId,
            int currentAdminUserId);
    }
}