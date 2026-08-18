using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Syntech.WebApp.Models;

namespace Syntech.WebApp.Data
{
    /// <summary>
    /// Creates the initial administrator account when the application
    /// starts and no administrator account exists.
    ///
    /// This seeder is intentionally idempotent:
    ///
    /// - If an Admin already exists, nothing is created.
    /// - If no Admin exists, a User record and corresponding Admin
    ///   record are created.
    ///
    /// The administrator password is hashed using the same
    /// PasswordHasher implementation used by AuthenticationService.
    ///
    /// IMPORTANT:
    ///
    /// Administrator credentials are supplied through application
    /// configuration/User Secrets and are NOT hard-coded into source code.
    /// </summary>
    public static class AdminSeeder
    {
        /// <summary>
        /// Seeds the initial administrator account.
        /// </summary>
        /// <param name="context">
        /// Application database context.
        /// </param>
        /// <param name="configuration">
        /// Application configuration containing the Admin seed settings.
        /// </param>
        public static async Task SeedAsync(
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            // -------------------------------------------------------------
            // Check whether an administrator already exists.
            //
            // If one exists, the application does not create another one.
            // -------------------------------------------------------------

            bool adminExists =
                await context.Admins.AnyAsync();

            if (adminExists)
            {
                return;
            }


            // -------------------------------------------------------------
            // Read the initial administrator configuration.
            //
            // These values will be stored in User Secrets during
            // development rather than committed to GitHub.
            // -------------------------------------------------------------

            string? adminName =
                configuration["AdminSeed:Name"];

            string? adminEmail =
                configuration["AdminSeed:Email"];

            string? adminPassword =
                configuration["AdminSeed:Password"];


            // -------------------------------------------------------------
            // Fail clearly if the required configuration has not been
            // supplied.
            // -------------------------------------------------------------

            if (string.IsNullOrWhiteSpace(adminName) ||
                string.IsNullOrWhiteSpace(adminEmail) ||
                string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException(
                    "Initial administrator configuration is missing. " +
                    "Configure AdminSeed:Name, AdminSeed:Email, and " +
                    "AdminSeed:Password using ASP.NET Core User Secrets.");
            }


            // -------------------------------------------------------------
            // Normalise the administrator information.
            // -------------------------------------------------------------

            adminName = adminName.Trim();
            adminEmail = adminEmail.Trim();


            // -------------------------------------------------------------
            // Prevent a duplicate email account.
            //
            // An administrator may not exist in dbo.Admin while a User
            // with the configured email already exists.
            // -------------------------------------------------------------

            User? existingUser =
                await context.Users
                    .SingleOrDefaultAsync(
                        user =>
                            user.Email == adminEmail);


            if (existingUser != null)
            {
                // ---------------------------------------------------------
                // The email already belongs to an account.
                //
                // We do not silently convert an existing Client or
                // Developer account into an Admin account during startup.
                //
                // This prevents accidental privilege escalation.
                // ---------------------------------------------------------

                throw new InvalidOperationException(
                    $"The configured AdminSeed email '{adminEmail}' " +
                    "already belongs to an existing user. " +
                    "Choose a different administrator email.");
            }


            // -------------------------------------------------------------
            // Create the User entity.
            // -------------------------------------------------------------

            var user = new User
            {
                Name = adminName,
                Email = adminEmail,
                Role = "Admin"
            };


            // -------------------------------------------------------------
            // Hash the initial administrator password.
            //
            // Plain-text passwords must never be stored in dbo.User.
            // -------------------------------------------------------------

            var passwordHasher =
                new PasswordHasher<User>();

            user.PasswordHash =
                passwordHasher.HashPassword(
                    user,
                    adminPassword);


            // -------------------------------------------------------------
            // Add the User entity.
            // -------------------------------------------------------------

            context.Users.Add(user);


            // -------------------------------------------------------------
            // Add the corresponding Admin role-specific record.
            //
            // Because Admin.UserId is both its primary key and foreign key,
            // the User navigation property establishes the relationship.
            // -------------------------------------------------------------

            context.Admins.Add(
                new Admin
                {
                    User = user
                });


            // -------------------------------------------------------------
            // Save both records.
            //
            // EF Core will insert the User first, obtain the generated
            // userID, and then insert the corresponding Admin record.
            // -------------------------------------------------------------

            await context.SaveChangesAsync();
        }
    }
}