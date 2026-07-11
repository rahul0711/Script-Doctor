using System;
using System.Collections.Generic;

namespace Pharma_Script.Models
{
    public class Prescription
    {
        public int PrescriptionID { get; set; }
        public int AppointmentID { get; set; }
        public int OrganizationID { get; set; }
        public int DoctorID { get; set; }
        public int PatientID { get; set; }
        public string PrescriptionNumber { get; set; } = string.Empty;
        public string? GeneralInstructions { get; set; }
        public DateTime? NextVisitDate { get; set; }
        public DateTime CreatedAt { get; set; }

        // Mapped dynamic collections
        public List<PrescriptionMedicine> Medicines { get; set; } = new List<PrescriptionMedicine>();
    }
}
