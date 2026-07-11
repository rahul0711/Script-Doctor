using System;
using System.Collections.Generic;

namespace Pharma_Script.Models
{
    public class Doctor
    {
        public int DoctorID { get; set; }
        public int UserID { get; set; }
        public int OrganizationID { get; set; }
        public int? BranchID { get; set; }
        public int? DepartmentID { get; set; }
        public string Qualification { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public string MedicalRegistrationNumber { get; set; } = string.Empty;
        public string? Biography { get; set; }
        public decimal ConsultationFee { get; set; }
        public decimal VideoConsultationFee { get; set; }
        public decimal VoiceConsultationFee { get; set; }
        public decimal PriorityConsultationFee { get; set; }
        public bool IsPriorityAvailable { get; set; }
        public DateTime? PriorityStartTime { get; set; }
        public DateTime? PriorityEndTime { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Joined fields for ease of display
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? ProfileImage { get; set; }
        public string? BranchName { get; set; }
        public string? DepartmentName { get; set; }
        public List<string> SpecializationNames { get; set; } = new List<string>();
        public List<int> SpecializationIDs { get; set; } = new List<int>();
    }
}
