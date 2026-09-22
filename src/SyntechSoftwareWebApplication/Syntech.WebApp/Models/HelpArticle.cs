using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Syntech.WebApp.Models
{
    [Table("HelpArticle")]
    public class HelpArticle
    {
        [Key]
        [Column("articleID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ArticleID { get; set; }

        [Required]
        [StringLength(200)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column("content", TypeName = "nvarchar(max)")]
        public string Content { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("category")]
        public string? Category { get; set; }

        [Required]
        [Column("createdDate")]
        public DateTime CreatedDate { get; set; }

        [Column("updatedDate")]
        public DateTime? UpdatedDate { get; set; }

        [Required]
        [Column("isActive")]
        public bool IsActive { get; set; }
    }
}
