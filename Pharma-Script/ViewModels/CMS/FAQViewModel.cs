using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.CMS
{
    public class FAQViewModel
    {
        public int FAQID { get; set; }

        [Required(ErrorMessage = "Question is required.")]
        [StringLength(500)]
        public string Question { get; set; } = string.Empty;

        [Required(ErrorMessage = "Answer is required.")]
        public string Answer { get; set; } = string.Empty;

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 1;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
