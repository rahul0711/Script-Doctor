using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.Consultation
{
    public class MedicineViewModel
    {
        [Required(ErrorMessage = "Medicine Name is required.")]
        [StringLength(255, ErrorMessage = "Medicine name is too long.")]
        public string MedicineName { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Strength is too long.")]
        public string? Strength { get; set; } // e.g. "500 mg"

        [StringLength(100, ErrorMessage = "Dosage details are too long.")]
        public string? Dosage { get; set; } // e.g. "1-0-1"

        public bool Morning { get; set; }
        public bool Afternoon { get; set; }
        public bool Night { get; set; }
        public bool BeforeFood { get; set; }
        public bool AfterFood { get; set; }

        [Range(1, 365, ErrorMessage = "Duration must be between 1 and 365 days.")]
        public int DurationDays { get; set; }

        [Range(1, 1000, ErrorMessage = "Quantity must be greater than 0.")]
        public int? Quantity { get; set; }

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? Remarks { get; set; }
    }
}
