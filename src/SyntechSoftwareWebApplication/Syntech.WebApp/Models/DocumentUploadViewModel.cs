using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Syntech.WebApp.Models
{
    public class DocumentUploadViewModel
    {
        [Required]
        [Display(Name = "Project")]
        public int ProjectID { get; set; }

        [Required]
        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;

        [Required]
        [Display(Name = "File")]
        public IFormFile? File { get; set; }
    }
}
