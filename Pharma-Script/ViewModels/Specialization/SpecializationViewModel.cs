using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.Specialization
{
    public class SpecializationViewModel
    {
        public int SpecializationID { get; set; }

        [Required(ErrorMessage = "Specialization Name is required.")]
        [StringLength(150, ErrorMessage = "Specialization Name cannot exceed 150 characters.")]
        [Display(Name = "Specialization Name")]
        public string SpecializationName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Display(Name = "Is Active?")]
        public bool IsActive { get; set; } = true;
    }
}
