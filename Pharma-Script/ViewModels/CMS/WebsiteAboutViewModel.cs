using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.CMS
{
    public class WebsiteAboutViewModel
    {
        [Display(Name = "About Us")]
        public string? AboutUs { get; set; }

        [Display(Name = "Mission")]
        public string? Mission { get; set; }

        [Display(Name = "Vision")]
        public string? Vision { get; set; }
    }
}
