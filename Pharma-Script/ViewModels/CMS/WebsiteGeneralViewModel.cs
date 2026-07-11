using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.CMS
{
    public class WebsiteGeneralViewModel
    {
        [Required(ErrorMessage = "Website Title is required.")]
        [StringLength(200, ErrorMessage = "Website Title cannot exceed 200 characters.")]
        [Display(Name = "Website Title")]
        public string WebsiteTitle { get; set; } = string.Empty;
    }
}
