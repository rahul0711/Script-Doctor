using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.CMS
{
    public class GalleryImageViewModel
    {
        public int GalleryID { get; set; }

        [StringLength(200)]
        [Display(Name = "Image Title")]
        public string? ImageTitle { get; set; }

        [Display(Name = "Image")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 1;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
