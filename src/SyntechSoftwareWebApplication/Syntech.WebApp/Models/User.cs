using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Syntech.WebApp.Models
{
    /// <summary>
    /// Represents a user stored in the application's dbo.User database table.
    ///
    /// This entity maps to the existing User table created by the project
    /// team.
    ///
    /// The User entity contains the account's core identity information,
    /// authentication information, role, and account status.
    /// </summary>
    [Table("User")]
    public class User
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user.
        ///
        /// The database generates this value automatically using
        /// IDENTITY(1,1).
        /// </summary>
        [Key]
        [Column("userID")]
        [DatabaseGenerated(
            DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }


        /// <summary>
        /// Gets or sets the user's full name.
        ///
        /// The database column is NVARCHAR(100) and does not allow NULL
        /// values.
        /// </summary>
        [Required]
        [StringLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the user's email address.
        ///
        /// The email address is unique and is used as the user's login
        /// identifier.
        /// </summary>
        [Required]
        [StringLength(255)]
        [EmailAddress]
        [Column("email")]
        public string Email { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the securely hashed version of the user's password.
        ///
        /// Plain-text passwords must never be stored in the database.
        /// PasswordHasher is responsible for generating and verifying
        /// password hashes.
        /// </summary>
        [Required]
        [StringLength(255)]
        [Column("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets the user's application role.
        ///
        /// Supported roles are:
        ///
        /// - Admin
        /// - Developer
        /// - Client
        /// </summary>
        [Required]
        [StringLength(20)]
        [Column("role")]
        public string Role { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets whether the user's account is currently active.
        ///
        /// Active accounts are permitted to authenticate.
        ///
        /// Deactivated accounts remain in the database but are prevented
        /// from logging in.
        /// </summary>
        [Required]
        [Column("isActive")]
        public bool IsActive { get; set; } = true;


        /// <summary>
        /// Navigation property for the optional Admin record associated
        /// with this user.
        /// </summary>
        public Admin? Admin { get; set; }


        /// <summary>
        /// Navigation property for the optional Developer record associated
        /// with this user.
        /// </summary>
        public Developer? Developer { get; set; }


        /// <summary>
        /// Navigation property for the optional Client record associated
        /// with this user.
        /// </summary>
        public Client? Client { get; set; }
    }
}