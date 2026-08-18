using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Syntech.WebApp.Models
{
    /// <summary>
    /// Represents the Admin role-specific record stored in dbo.Admin.
    ///
    /// The Admin table uses userID as both its primary key and foreign key
    /// to dbo.User.
    /// </summary>
    [Table("Admin")]
    public class Admin
    {
        /// <summary>
        /// Gets or sets the user identifier.
        ///
        /// This is both the primary key of the Admin table and a foreign key
        /// referencing dbo.User(userID).
        /// </summary>
        [Key]
        [Column("userID")]
        public int UserId { get; set; }

        /// <summary>
        /// Navigation property linking the Admin record to its User record.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
    }
}