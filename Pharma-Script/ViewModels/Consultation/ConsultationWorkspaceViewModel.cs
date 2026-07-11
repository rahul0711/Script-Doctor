using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.Consultation
{
    public class ConsultationWorkspaceViewModel
    {
        [Required]
        public int AppointmentID { get; set; }

        public string? ClinicalNotes { get; set; }

        [Required(ErrorMessage = "Diagnosis details are required.")]
        public string Diagnosis { get; set; } = string.Empty;

        public string? Advice { get; set; }

        // Prescription options
        public bool CreatePrescription { get; set; }
        
        public string? GeneralInstructions { get; set; }

        [DataType(DataType.Date)]
        public DateTime? NextVisitDate { get; set; }

        public List<MedicineViewModel> Medicines { get; set; } = new List<MedicineViewModel>();
    }
}
