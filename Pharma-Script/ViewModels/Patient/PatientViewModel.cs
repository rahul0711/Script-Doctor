using System;
using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.Patient
{
    public class PatientViewModel
    {
        public int PatientID { get; set; }
        public int UserID { get; set; }
        public int OrganizationID { get; set; }

        [Display(Name = "Branch")]
        public int? BranchID { get; set; }

        [Required(ErrorMessage = "Date of Birth is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-20);

        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; } = string.Empty; // Male, Female, Other

        [Display(Name = "Blood Group")]
        public string? BloodGroup { get; set; } // A+, A-, etc.

        [Range(0, 300, ErrorMessage = "Height must be a valid number in cm.")]
        public decimal? Height { get; set; }

        [Range(0, 500, ErrorMessage = "Weight must be a valid number in kg.")]
        public decimal? Weight { get; set; }

        [Display(Name = "Emergency Contact Name")]
        [StringLength(200, ErrorMessage = "Contact name cannot exceed 200 characters.")]
        public string? EmergencyContactName { get; set; }

        [Display(Name = "Emergency Contact Number")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(20, ErrorMessage = "Contact phone cannot exceed 20 characters.")]
        public string? EmergencyContactNumber { get; set; }

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
        public string? Address { get; set; }

        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "State cannot exceed 100 characters.")]
        public string? State { get; set; }

        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters.")]
        public string? Country { get; set; }

        [StringLength(15, ErrorMessage = "Pincode cannot exceed 15 characters.")]
        public string? Pincode { get; set; }

        [Display(Name = "Is Active?")]
        public bool IsActive { get; set; } = true;

        // User specific fields
        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(100, ErrorMessage = "First Name cannot exceed 100 characters.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Last Name cannot exceed 100 characters.")]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        public string Phone { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }
    }
}
