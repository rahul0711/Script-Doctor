using System;

namespace Pharma_Script.Models
{
    public class Patient
    {
        public int PatientID { get; set; }
        public int UserID { get; set; }
        public int OrganizationID { get; set; }
        public int? BranchID { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty; // Male, Female, Other
        public string? BloodGroup { get; set; } // A+, A-, etc.
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? Pincode { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Joined fields for display
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? BranchName { get; set; }
        public string? OrganizationName { get; set; }
    }
}
