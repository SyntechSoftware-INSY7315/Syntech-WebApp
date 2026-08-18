using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Syntech.WebApp.Data;
using Syntech.WebApp.Models;

namespace Syntech.WebApp.Services
{
    /// <summary>
    /// Provides authentication and account-management business logic
    /// for the Syntech application.
    ///
    /// Responsibilities include:
    ///
    /// - User authentication.
    /// - Public registration.
    /// - Administrator-created accounts.
    /// - Administrator user editing.
    /// - Account activation/deactivation.
    /// - Administrator user deletion.
    /// - Password changes.
    /// - Secure password hashing.
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Provides secure password hashing and verification.
        /// </summary>
        private readonly PasswordHasher<User> _passwordHasher;


        /// <summary>
        /// Initializes the authentication service.
        /// </summary>
        public AuthenticationService(
            ApplicationDbContext context)
        {
            _context = context;

            _passwordHasher =
                new PasswordHasher<User>();
        }


        // =====================================================================
        // EMAIL CHECK
        // =====================================================================

        /// <inheritdoc />
        public async Task<bool> EmailExistsAsync(
            string email)
        {
            string normalisedEmail =
                email.Trim();

            return await _context.Users
                .AnyAsync(
                    user =>
                        user.Email == normalisedEmail);
        }


        // =====================================================================
        // AUTHENTICATION
        // =====================================================================

        /// <inheritdoc />
        public async Task<User?> AuthenticateAsync(
            string email,
            string password)
        {
            string normalisedEmail =
                email.Trim();

            User? user =
                await _context.Users
                    .SingleOrDefaultAsync(
                        existingUser =>
                            existingUser.Email ==
                            normalisedEmail);

            if (user == null)
            {
                return null;
            }

            // Deactivated accounts cannot authenticate.
            if (!user.IsActive)
            {
                return null;
            }

            PasswordVerificationResult result =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    password);

            if (result ==
                    PasswordVerificationResult.Success ||
                result ==
                    PasswordVerificationResult.SuccessRehashNeeded)
            {
                return user;
            }

            return null;
        }


        // =====================================================================
        // PUBLIC REGISTRATION
        // =====================================================================

        /// <inheritdoc />
        public async Task<User> RegisterAsync(
            string name,
            string email,
            string password,
            string role)
        {
            string normalisedName =
                name.Trim();

            string normalisedEmail =
                email.Trim();

            string normalisedRole =
                NormaliseRole(role.Trim());


            if (string.IsNullOrWhiteSpace(
                    normalisedName))
            {
                throw new ArgumentException(
                    "Name cannot be empty.",
                    nameof(name));
            }


            if (string.IsNullOrWhiteSpace(
                    normalisedEmail))
            {
                throw new ArgumentException(
                    "Email address cannot be empty.",
                    nameof(email));
            }


            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException(
                    "Password cannot be empty.",
                    nameof(password));
            }


            if (await EmailExistsAsync(
                    normalisedEmail))
            {
                throw new InvalidOperationException(
                    "An account with this email address already exists.");
            }


            var user = new User
            {
                Name = normalisedName,
                Email = normalisedEmail,
                Role = normalisedRole,
                IsActive = true
            };


            user.PasswordHash =
                _passwordHasher.HashPassword(
                    user,
                    password);


            _context.Users.Add(user);


            AddRoleSpecificRecord(
                user,
                normalisedRole);


            await _context.SaveChangesAsync();

            return user;
        }


        // =====================================================================
        // ADMIN USER CREATION
        // =====================================================================

        /// <inheritdoc />
        public async Task<User> CreateUserByAdminAsync(
            string name,
            string email,
            string password,
            string role)
        {
            string normalisedName =
                name.Trim();

            string normalisedEmail =
                email.Trim();

            string normalisedRole =
                NormaliseRole(role.Trim());


            if (string.IsNullOrWhiteSpace(
                    normalisedName))
            {
                throw new ArgumentException(
                    "Name cannot be empty.",
                    nameof(name));
            }


            if (string.IsNullOrWhiteSpace(
                    normalisedEmail))
            {
                throw new ArgumentException(
                    "Email address cannot be empty.",
                    nameof(email));
            }


            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException(
                    "Password cannot be empty.",
                    nameof(password));
            }


            if (await EmailExistsAsync(
                    normalisedEmail))
            {
                throw new InvalidOperationException(
                    "An account with this email address already exists.");
            }


            var user = new User
            {
                Name = normalisedName,
                Email = normalisedEmail,
                Role = normalisedRole,
                IsActive = true
            };


            user.PasswordHash =
                _passwordHasher.HashPassword(
                    user,
                    password);


            _context.Users.Add(user);


            AddRoleSpecificRecord(
                user,
                normalisedRole);


            await _context.SaveChangesAsync();

            return user;
        }


        // =====================================================================
        // ADMIN USER EDITING
        // =====================================================================

        /// <inheritdoc />
        public async Task<User> UpdateUserByAdminAsync(
            int userId,
            string name,
            string email,
            string role)
        {
            string normalisedName =
                name.Trim();

            string normalisedEmail =
                email.Trim();

            string normalisedRole =
                NormaliseRole(role.Trim());


            User? user =
                await _context.Users
                    .SingleOrDefaultAsync(
                        existingUser =>
                            existingUser.UserId ==
                            userId);


            if (user == null)
            {
                throw new KeyNotFoundException(
                    "The requested user could not be found.");
            }


            if (string.IsNullOrWhiteSpace(
                    normalisedName))
            {
                throw new ArgumentException(
                    "Name cannot be empty.",
                    nameof(name));
            }


            if (string.IsNullOrWhiteSpace(
                    normalisedEmail))
            {
                throw new ArgumentException(
                    "Email address cannot be empty.",
                    nameof(email));
            }


            bool emailBelongsToAnotherUser =
                await _context.Users
                    .AnyAsync(
                        existingUser =>
                            existingUser.Email ==
                                normalisedEmail &&
                            existingUser.UserId != userId);


            if (emailBelongsToAnotherUser)
            {
                throw new InvalidOperationException(
                    "Another account already uses this email address.");
            }


            user.Name =
                normalisedName;

            user.Email =
                normalisedEmail;


            if (!user.Role.Equals(
                    normalisedRole,
                    StringComparison.OrdinalIgnoreCase))
            {
                RemoveRoleSpecificRecord(user);

                user.Role =
                    normalisedRole;

                AddRoleSpecificRecord(
                    user,
                    normalisedRole);
            }


            await _context.SaveChangesAsync();

            return user;
        }


        // =====================================================================
        // ACTIVATE / DEACTIVATE
        // =====================================================================

        /// <inheritdoc />
        public async Task<User> SetUserActiveStatusAsync(
            int userId,
            bool isActive)
        {
            User? user =
                await _context.Users
                    .SingleOrDefaultAsync(
                        existingUser =>
                            existingUser.UserId ==
                            userId);


            if (user == null)
            {
                throw new KeyNotFoundException(
                    "The requested user could not be found.");
            }


            user.IsActive =
                isActive;


            await _context.SaveChangesAsync();

            return user;
        }


        // =====================================================================
        // DELETE USER
        // =====================================================================

        /// <inheritdoc />
        public async Task DeleteUserByAdminAsync(
            int userId,
            int currentAdminUserId)
        {
            // -------------------------------------------------------------
            // Prevent an administrator from deleting their own account.
            // -------------------------------------------------------------

            if (userId == currentAdminUserId)
            {
                throw new InvalidOperationException(
                    "You cannot delete your own administrator account.");
            }


            // -------------------------------------------------------------
            // Find the user.
            // -------------------------------------------------------------

            User? user =
                await _context.Users
                    .SingleOrDefaultAsync(
                        existingUser =>
                            existingUser.UserId ==
                            userId);


            if (user == null)
            {
                throw new KeyNotFoundException(
                    "The requested user could not be found.");
            }


            // -------------------------------------------------------------
            // Remove the role-specific record first.
            //
            // This makes the deletion explicit and ensures the correct
            // role table is cleaned up.
            // -------------------------------------------------------------

            RemoveRoleSpecificRecord(user);


            // -------------------------------------------------------------
            // Remove the base User record.
            // -------------------------------------------------------------

            _context.Users.Remove(user);


            // -------------------------------------------------------------
            // Save both operations.
            // -------------------------------------------------------------

            await _context.SaveChangesAsync();
        }


        // =====================================================================
        // CHANGE PASSWORD
        // =====================================================================

        /// <inheritdoc />
        public async Task<bool> ChangePasswordAsync(
            int userId,
            string currentPassword,
            string newPassword)
        {
            User? user =
                await _context.Users
                    .SingleOrDefaultAsync(
                        existingUser =>
                            existingUser.UserId ==
                            userId);


            if (user == null)
            {
                return false;
            }


            PasswordVerificationResult currentPasswordResult =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    currentPassword);


            if (currentPasswordResult !=
                    PasswordVerificationResult.Success &&
                currentPasswordResult !=
                    PasswordVerificationResult.SuccessRehashNeeded)
            {
                return false;
            }


            PasswordVerificationResult samePasswordResult =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    newPassword);


            if (samePasswordResult ==
                    PasswordVerificationResult.Success ||
                samePasswordResult ==
                    PasswordVerificationResult.SuccessRehashNeeded)
            {
                throw new InvalidOperationException(
                    "The new password must be different from your current password.");
            }


            user.PasswordHash =
                _passwordHasher.HashPassword(
                    user,
                    newPassword);


            await _context.SaveChangesAsync();

            return true;
        }


        // =====================================================================
        // ROLE VALIDATION
        // =====================================================================

        /// <summary>
        /// Validates and normalises an application role.
        /// </summary>
        private static string NormaliseRole(
            string role)
        {
            if (role.Equals(
                    "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Admin";
            }


            if (role.Equals(
                    "Developer",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Developer";
            }


            if (role.Equals(
                    "Client",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Client";
            }


            throw new ArgumentException(
                "The selected role is not valid.",
                nameof(role));
        }


        // =====================================================================
        // ROLE RECORD CREATION
        // =====================================================================

        /// <summary>
        /// Creates the role-specific record associated with a User.
        /// </summary>
        private void AddRoleSpecificRecord(
            User user,
            string role)
        {
            switch (role)
            {
                case "Admin":

                    _context.Admins.Add(
                        new Admin
                        {
                            User = user
                        });

                    break;


                case "Developer":

                    _context.Developers.Add(
                        new Developer
                        {
                            User = user
                        });

                    break;


                case "Client":

                    _context.Clients.Add(
                        new Client
                        {
                            User = user
                        });

                    break;
            }
        }


        // =====================================================================
        // ROLE RECORD REMOVAL
        // =====================================================================

        /// <summary>
        /// Removes the role-specific record associated with a User.
        /// </summary>
        private void RemoveRoleSpecificRecord(
            User user)
        {
            switch (user.Role)
            {
                case "Admin":

                    Admin? admin =
                        _context.Admins
                            .Local
                            .FirstOrDefault(
                                record =>
                                    record.UserId ==
                                    user.UserId);


                    if (admin == null)
                    {
                        admin =
                            _context.Admins
                                .Find(user.UserId);
                    }


                    if (admin != null)
                    {
                        _context.Admins.Remove(admin);
                    }

                    break;


                case "Developer":

                    Developer? developer =
                        _context.Developers
                            .Local
                            .FirstOrDefault(
                                record =>
                                    record.UserId ==
                                    user.UserId);


                    if (developer == null)
                    {
                        developer =
                            _context.Developers
                                .Find(user.UserId);
                    }


                    if (developer != null)
                    {
                        _context.Developers.Remove(
                            developer);
                    }

                    break;


                case "Client":

                    Client? client =
                        _context.Clients
                            .Local
                            .FirstOrDefault(
                                record =>
                                    record.UserId ==
                                    user.UserId);


                    if (client == null)
                    {
                        client =
                            _context.Clients
                                .Find(user.UserId);
                    }


                    if (client != null)
                    {
                        _context.Clients.Remove(
                            client);
                    }

                    break;
            }
        }
    }
}