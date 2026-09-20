using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Syntech.WebApp.Models
{
    [Table("Document")]
    public class Document
    {
        [Key]
        [Column("documentID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DocumentID { get; set; }

        [Required]
        [StringLength(255)]
        [Column("fileName")]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column("originalFileName")]
        public string OriginalFileName { get; set; } = string.Empty;

        [StringLength(20)]
        [Column("fileExtension")]
        public string? FileExtension { get; set; }

        [StringLength(100)]
        [Column("fileType")]
        public string? FileType { get; set; }

        [Required]
        [StringLength(500)]
        [Column("filePath")]
        public string FilePath { get; set; } = string.Empty;

        [Column("fileSize")]
        public long? FileSize { get; set; }

        [Required]
        [Column("uploadedBy")]
        public int UploadedBy { get; set; }

        [Column("uploadDate")]
        public DateTime? UploadDate { get; set; }

        [Required]
        [Column("projectID")]
        public int ProjectID { get; set; }

        [Required]
        [StringLength(30)]
        [Column("category")]
        public string Category { get; set; } = string.Empty;



        [ForeignKey(nameof(UploadedBy))]
        public User? Uploader { get; set; }

        [ForeignKey(nameof(ProjectID))]
        public Project? Project { get; set; }
    }
}
