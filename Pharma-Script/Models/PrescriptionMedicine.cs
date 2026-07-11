namespace Pharma_Script.Models
{
    public class PrescriptionMedicine
    {
        public int PrescriptionMedicineID { get; set; }
        public int PrescriptionID { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string? Strength { get; set; }
        public string? Dosage { get; set; }
        public bool Morning { get; set; }
        public bool Afternoon { get; set; }
        public bool Night { get; set; }
        public bool BeforeFood { get; set; }
        public bool AfterFood { get; set; }
        public int DurationDays { get; set; }
        public int? Quantity { get; set; }
        public string? Remarks { get; set; }
    }
}
