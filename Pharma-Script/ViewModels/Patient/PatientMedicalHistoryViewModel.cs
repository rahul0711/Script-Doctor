using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.Patient
{
    public class PatientMedicalHistoryViewModel
    {
        public int MedicalHistoryID { get; set; }

        [Required(ErrorMessage = "Patient is required.")]
        public int PatientID { get; set; }

        [Display(Name = "Diabetes")]
        public bool Diabetes { get; set; }

        [Display(Name = "High Blood Pressure")]
        public bool BloodPressure { get; set; }

        [Display(Name = "Heart Disease")]
        public bool HeartDisease { get; set; }

        [Display(Name = "Asthma")]
        public bool Asthma { get; set; }

        [Display(Name = "Thyroid Condition")]
        public bool Thyroid { get; set; }

        [Display(Name = "Allergies")]
        public string? Allergies { get; set; }

        [Display(Name = "Current Medications")]
        public string? CurrentMedications { get; set; }

        [Display(Name = "Past Surgeries")]
        public string? PastSurgeries { get; set; }

        [Display(Name = "Family Medical History")]
        public string? FamilyMedicalHistory { get; set; }

        [Display(Name = "Other Medical Conditions")]
        public string? OtherMedicalConditions { get; set; }
    }
}
