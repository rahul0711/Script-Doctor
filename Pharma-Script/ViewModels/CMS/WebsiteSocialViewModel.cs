using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.CMS
{
    public class WebsiteSocialViewModel
    {
        [Url(ErrorMessage = "Enter a valid URL.")]
        [StringLength(300)]
        [Display(Name = "Facebook URL")]
        public string? FacebookURL { get; set; }

        [Url(ErrorMessage = "Enter a valid URL.")]
        [StringLength(300)]
        [Display(Name = "Instagram URL")]
        public string? InstagramURL { get; set; }

        [Url(ErrorMessage = "Enter a valid URL.")]
        [StringLength(300)]
        [Display(Name = "LinkedIn URL")]
        public string? LinkedInURL { get; set; }

        [Url(ErrorMessage = "Enter a valid URL.")]
        [StringLength(300)]
        [Display(Name = "Twitter / X URL")]
        public string? TwitterURL { get; set; }

        [Url(ErrorMessage = "Enter a valid URL.")]
        [StringLength(300)]
        [Display(Name = "YouTube URL")]
        public string? YouTubeURL { get; set; }
    }
}
