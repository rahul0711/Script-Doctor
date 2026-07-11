using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.CMS
{
    public class WebsiteFooterViewModel
    {
        [Display(Name = "Footer Text")]
        public string? FooterText { get; set; }
    }
}
