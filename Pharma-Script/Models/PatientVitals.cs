using System;

namespace Pharma_Script.Models
{
    public class PatientVitals
    {
        public int VitalID { get; set; }
        public int PatientID { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string? BloodPressure { get; set; }
        public int? HeartRate { get; set; }
        public decimal? Temperature { get; set; }
        public int? OxygenLevel { get; set; }
        public string? BloodSugar { get; set; }
        public decimal? BMI { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.Now;
    }
}
