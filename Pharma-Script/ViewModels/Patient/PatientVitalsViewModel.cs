using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.Patient
{
    public class PatientVitalsViewModel
    {
        public int VitalID { get; set; }

        [Required(ErrorMessage = "Patient is required.")]
        public int PatientID { get; set; }

        [Range(10, 250, ErrorMessage = "Height must be between 10 cm and 250 cm.")]
        [Display(Name = "Height (cm)")]
        public decimal? Height { get; set; }

        [Range(1, 500, ErrorMessage = "Weight must be between 1 kg and 500 kg.")]
        [Display(Name = "Weight (kg)")]
        public decimal? Weight { get; set; }

        [StringLength(20, ErrorMessage = "Blood Pressure format must be systolic/diastolic (e.g. 120/80).")]
        [RegularExpression(@"^\d{2,3}/\d{2,3}$", ErrorMessage = "Blood pressure must be in Systolic/Diastolic format (e.g., 120/80).")]
        [Display(Name = "Blood Pressure (mmHg)")]
        public string? BloodPressure { get; set; }

        [Range(30, 250, ErrorMessage = "Heart rate must be between 30 and 250 bpm.")]
        [Display(Name = "Heart Rate (bpm)")]
        public int? HeartRate { get; set; }

        [Range(90, 115, ErrorMessage = "Temperature must be between 90°F and 115°F.")]
        [Display(Name = "Temperature (°F)")]
        public decimal? Temperature { get; set; }

        [Range(50, 100, ErrorMessage = "Oxygen level must be between 50% and 100%.")]
        [Display(Name = "Oxygen Level (SpO2 %)")]
        public int? OxygenLevel { get; set; }

        [StringLength(20, ErrorMessage = "Blood sugar value cannot exceed 20 characters.")]
        [Display(Name = "Blood Sugar (mg/dL)")]
        public string? BloodSugar { get; set; }

        [Display(Name = "BMI")]
        public decimal? BMI { get; set; }
    }
}
