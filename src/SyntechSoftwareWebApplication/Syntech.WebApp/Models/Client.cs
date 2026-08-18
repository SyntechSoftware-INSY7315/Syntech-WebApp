using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Syntech.WebApp.Models
{
    /// <summary>
    /// Represents the Client role-specific record stored in dbo.Client.
    ///
    /// The Client table uses userID as both its primary key and foreign key
    /// to dbo.User.
    /// </summary>
    [Table("Client")]
    public class Client
    {
        /// <summary>
        /// Gets or sets the user identifier.
        ///
        /// This is both the primary key of the Client table and a foreign key
        /// referencing dbo.User(userID).
        /// </summary>
        [Key]
        [Column("userID")]
        public int UserId { get; set; }

        /// <summary>
        /// Navigation property linking the Client record to its User record.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
    }
}