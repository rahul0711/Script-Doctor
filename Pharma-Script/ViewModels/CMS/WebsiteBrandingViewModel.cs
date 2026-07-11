using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.CMS
{
    public class WebsiteBrandingViewModel
    {
        [RegularExpression("^#([0-9a-fA-F]{6}|[0-9a-fA-F]{3})$", ErrorMessage = "Enter a valid hex color (e.g. #2563EB).")]
        [Display(Name = "Primary Color")]
        public string? PrimaryColor { get; set; }

        [RegularExpression("^#([0-9a-fA-F]{6}|[0-9a-fA-F]{3})$", ErrorMessage = "Enter a valid hex color (e.g. #059669).")]
        [Display(Name = "Secondary Color")]
        public string? SecondaryColor { get; set; }

        public string? ExistingLogo { get; set; }
        public string? ExistingFavicon { get; set; }

        [Display(Name = "Website Logo")]
        public IFormFile? Logo { get; set; }

        [Display(Name = "Favicon")]
        public IFormFile? Favicon { get; set; }
    }
}
