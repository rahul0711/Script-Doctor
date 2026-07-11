using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.CMS
{
    public class ServiceViewModel
    {
        public int ServiceID { get; set; }

        [Required(ErrorMessage = "Service Name is required.")]
        [StringLength(200)]
        [Display(Name = "Service Name")]
        public string ServiceName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ExistingServiceImage { get; set; }

        [Display(Name = "Service Image")]
        public IFormFile? ServiceImage { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
