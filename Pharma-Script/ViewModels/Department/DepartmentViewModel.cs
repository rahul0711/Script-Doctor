using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.Department
{
    public class DepartmentViewModel
    {
        public int DepartmentID { get; set; }

        [Required(ErrorMessage = "Organization is required.")]
        [Display(Name = "Organization")]
        public int OrganizationID { get; set; }

        [Required(ErrorMessage = "Department Name is required.")]
        [StringLength(150, ErrorMessage = "Department Name cannot exceed 150 characters.")]
        [Display(Name = "Department Name")]
        public string DepartmentName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Display(Name = "Is Active?")]
        public bool IsActive { get; set; } = true;
    }
}
