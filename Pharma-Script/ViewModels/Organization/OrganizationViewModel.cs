using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.Organization
{
    public class OrganizationViewModel
    {
        public int OrganizationID { get; set; }

        [Required(ErrorMessage = "Organization Name is required.")]
        [StringLength(200, ErrorMessage = "Organization Name cannot exceed 200 characters.")]
        [Display(Name = "Organization Name")]
        public string OrganizationName { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "Website Slug cannot exceed 150 characters.")]
        [RegularExpression("^[a-z0-9]+(-[a-z0-9]+)*$", ErrorMessage = "Slug can only contain lowercase letters, numbers, and hyphens (e.g. abc-hospital).")]
        [Display(Name = "Website Slug")]
        public string? OrganizationSlug { get; set; }

        [Required(ErrorMessage = "Organization Type is required.")]
        [StringLength(50, ErrorMessage = "Organization Type cannot exceed 50 characters.")]
        [Display(Name = "Organization Type")]
        public string OrganizationType { get; set; } = string.Empty; // Clinic, Hospital, Dental Clinic, Diagnostic Center

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters.")]
        public string Phone { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid alternate phone number format.")]
        [StringLength(20, ErrorMessage = "Alternate phone cannot exceed 20 characters.")]
        [Display(Name = "Alternate Phone")]
        public string? AlternatePhone { get; set; }

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

        [StringLength(50, ErrorMessage = "GST Number cannot exceed 50 characters.")]
        [Display(Name = "GST Number")]
        public string? GSTNumber { get; set; }

        [StringLength(100, ErrorMessage = "License Number cannot exceed 100 characters.")]
        [Display(Name = "License Number")]
        public string? LicenseNumber { get; set; }

        [Display(Name = "Is Active?")]
        public bool IsActive { get; set; } = true;
    }
}
