using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.CMS
{
    public class HeroSectionViewModel
    {
        public int HeroSectionID { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Subtitle")]
        public string? Subtitle { get; set; }

        public string? ExistingBannerImage { get; set; }

        [Display(Name = "Banner Image")]
        public IFormFile? BannerImage { get; set; }

        [StringLength(100)]
        [Display(Name = "Button Text")]
        public string? ButtonText { get; set; }

        [StringLength(300)]
        [Display(Name = "Button URL")]
        public string? ButtonURL { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
