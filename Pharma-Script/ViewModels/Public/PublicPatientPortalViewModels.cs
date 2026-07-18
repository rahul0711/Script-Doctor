using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.Public
{
    // One entry in the patient's medical timeline, anchored to a single appointment.
    public class PatientTimelineEntry
    {
        public Pharma_Script.Models.Appointment Appointment { get; set; } = null!;

        // Doctor note — only Diagnosis + Advice are shown to patient (ClinicalNotes is private).
        public Pharma_Script.Models.DoctorNote? Note { get; set; }

        // Prescription header + medicines (null if no prescription was issued)
        public Pharma_Script.Models.Prescription? Prescription { get; set; }
        public List<Pharma_Script.Models.PrescriptionMedicine> Medicines { get; set; } = new();

        // Follow-up scheduled after this appointment (null if none)
        public Pharma_Script.Models.FollowUp? FollowUp { get; set; }
    }

    // Full "My History" view model
    public class PublicPatientHistoryViewModel
    {
        public Pharma_Script.Models.PublicTenant Tenant { get; set; } = null!;
        public Pharma_Script.Models.Patient Patient { get; set; } = null!;
        public List<PatientTimelineEntry> Timeline { get; set; } = new();
        public Pharma_Script.Models.PatientVitals? LatestVitals { get; set; }
    }

    // "My Appointments" view model — paginated list
    public class PublicMyAppointmentsViewModel
    {
        public Pharma_Script.Models.PublicTenant Tenant { get; set; } = null!;
        public Pharma_Script.Models.Patient Patient { get; set; } = null!;
        public IEnumerable<Pharma_Script.Models.Appointment> Appointments { get; set; }
            = new List<Pharma_Script.Models.Appointment>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalItems { get; set; }
        public int PageSize { get; set; } = 10;
        public string? StatusFilter { get; set; }
    }

    // "My Profile" view model
    public class PublicMyProfileViewModel
    {
        public Pharma_Script.Models.PublicTenant Tenant { get; set; } = null!;
        public Pharma_Script.Models.Patient Patient { get; set; } = null!;
        public Pharma_Script.Models.PatientVitals? LatestVitals { get; set; }
    }

    // "Edit Profile" form model — the patient's own self-service subset of PatientViewModel.
    // OrganizationID, BranchID, and IsActive are never editable here.
    public class PublicEditProfileViewModel
    {
        public Pharma_Script.Models.PublicTenant Tenant { get; set; } = null!;

        public int PatientID { get; set; }
        public int UserID { get; set; }

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

        [Required(ErrorMessage = "Date of Birth is required.")]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; } = string.Empty;

        [Display(Name = "Blood Group")]
        public string? BloodGroup { get; set; }

        [Range(0, 300, ErrorMessage = "Enter a valid height in cm.")]
        [Display(Name = "Height (cm)")]
        public decimal? Height { get; set; }

        [Range(0, 500, ErrorMessage = "Enter a valid weight in kg.")]
        [Display(Name = "Weight (kg)")]
        public decimal? Weight { get; set; }

        [StringLength(100)]
        [Display(Name = "Emergency Contact Name")]
        public string? EmergencyContactName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        [Display(Name = "Emergency Contact Number")]
        public string? EmergencyContactNumber { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(20)]
        public string? Pincode { get; set; }

        // Optional password change — leave blank to keep the current password.
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation do not match.")]
        [Display(Name = "Confirm New Password")]
        public string? ConfirmNewPassword { get; set; }
    }
}
