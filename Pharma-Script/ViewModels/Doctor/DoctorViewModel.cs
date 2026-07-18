using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.Doctor
{
    public class DoctorViewModel
    {
        public int DoctorID { get; set; }
        public int UserID { get; set; }
        public int OrganizationID { get; set; }

        [Display(Name = "Profile Image")]
        public IFormFile? ProfileImageFile { get; set; }

        public string? ExistingProfileImage { get; set; }

        [Display(Name = "Branch")]
        public int? BranchID { get; set; }

        [Display(Name = "Department")]
        public int? DepartmentID { get; set; }

        [Required(ErrorMessage = "Qualification is required.")]
        [StringLength(250, ErrorMessage = "Qualification cannot exceed 250 characters.")]
        public string Qualification { get; set; } = string.Empty;

        [Required(ErrorMessage = "Experience (in years) is required.")]
        [Range(0, 100, ErrorMessage = "Experience must be between 0 and 100 years.")]
        [Display(Name = "Experience (Years)")]
        public int ExperienceYears { get; set; }

        [Required(ErrorMessage = "Medical Registration Number is required.")]
        [StringLength(100, ErrorMessage = "Medical Registration Number cannot exceed 100 characters.")]
        [Display(Name = "Medical Registration Number")]
        public string MedicalRegistrationNumber { get; set; } = string.Empty;

        public string? Biography { get; set; }

        [Required(ErrorMessage = "Consultation Fee is required.")]
        [Range(0.0, 100000.0, ErrorMessage = "Fee must be greater than or equal to 0.")]
        [Display(Name = "Consultation Fee (INR)")]
        public decimal ConsultationFee { get; set; }

        [Required(ErrorMessage = "Video Consultation Fee is required.")]
        [Range(0.0, 100000.0, ErrorMessage = "Fee must be greater than or equal to 0.")]
        [Display(Name = "Video Consultation Fee (INR)")]
        public decimal VideoConsultationFee { get; set; }

        [Required(ErrorMessage = "Voice Consultation Fee is required.")]
        [Range(0.0, 100000.0, ErrorMessage = "Fee must be greater than or equal to 0.")]
        [Display(Name = "Voice Consultation Fee (INR)")]
        public decimal VoiceConsultationFee { get; set; }

        [Required(ErrorMessage = "Priority Consultation Fee is required.")]
        [Range(0.0, 100000.0, ErrorMessage = "Fee must be greater than or equal to 0.")]
        [Display(Name = "Priority Consultation Fee (INR)")]
        public decimal PriorityConsultationFee { get; set; }

        [Display(Name = "Is Priority Consultation Available?")]
        public bool IsPriorityAvailable { get; set; }

        [Display(Name = "Priority Start Time")]
        [DataType(DataType.DateTime)]
        public DateTime? PriorityStartTime { get; set; }

        [Display(Name = "Priority End Time")]
        [DataType(DataType.DateTime)]
        public DateTime? PriorityEndTime { get; set; }

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

        [Required(ErrorMessage = "Please select at least one specialization.")]
        [Display(Name = "Specializations")]
        public List<int> SpecializationIDs { get; set; } = new List<int>();
    }
}
