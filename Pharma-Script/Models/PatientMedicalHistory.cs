using System;

namespace Pharma_Script.Models
{
    public class PatientMedicalHistory
    {
        public int MedicalHistoryID { get; set; }
        public int PatientID { get; set; }
        public bool Diabetes { get; set; }
        public bool BloodPressure { get; set; }
        public bool HeartDisease { get; set; }
        public bool Asthma { get; set; }
        public bool Thyroid { get; set; }
        public string? Allergies { get; set; }
        public string? CurrentMedications { get; set; }
        public string? PastSurgeries { get; set; }
        public string? FamilyMedicalHistory { get; set; }
        public string? OtherMedicalConditions { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
