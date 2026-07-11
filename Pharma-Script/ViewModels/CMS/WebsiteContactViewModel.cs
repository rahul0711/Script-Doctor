using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.CMS
{
    public class WebsiteContactViewModel
    {
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [StringLength(150)]
        [Display(Name = "Contact Email")]
        public string? ContactEmail { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(20)]
        [Display(Name = "Contact Phone")]
        public string? ContactPhone { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(20)]
        [Display(Name = "Emergency Phone")]
        public string? EmergencyPhone { get; set; }

        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Display(Name = "Google Map Embed URL")]
        public string? GoogleMapEmbed { get; set; }
    }
}
