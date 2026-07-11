using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.Branch
{
    public class BranchViewModel
    {
        public int BranchID { get; set; }

        [Required(ErrorMessage = "Organization is required.")]
        [Display(Name = "Organization")]
        public int OrganizationID { get; set; }

        [Required(ErrorMessage = "Branch Name is required.")]
        [StringLength(200, ErrorMessage = "Branch Name cannot exceed 200 characters.")]
        [Display(Name = "Branch Name")]
        public string BranchName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters.")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Address Line 1 is required.")]
        [StringLength(250, ErrorMessage = "Address Line 1 cannot exceed 250 characters.")]
        [Display(Name = "Address Line 1")]
        public string AddressLine1 { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Address Line 2 cannot exceed 250 characters.")]
        [Display(Name = "Address Line 2")]
        public string? AddressLine2 { get; set; }

        [Required(ErrorMessage = "City is required.")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required.")]
        [StringLength(100, ErrorMessage = "State cannot exceed 100 characters.")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required.")]
        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters.")]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pincode is required.")]
        [StringLength(15, ErrorMessage = "Pincode cannot exceed 15 characters.")]
        public string Pincode { get; set; } = string.Empty;

        [Display(Name = "Is Main Branch?")]
        public bool IsMainBranch { get; set; }

        [Display(Name = "Is Active?")]
        public bool IsActive { get; set; } = true;
    }
}
