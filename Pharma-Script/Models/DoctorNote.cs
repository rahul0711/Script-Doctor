using System;

namespace Pharma_Script.Models
{
    public class DoctorNote
    {
        public int NoteID { get; set; }
        public int AppointmentID { get; set; }
        public int OrganizationID { get; set; }
        public int DoctorID { get; set; }
        public int PatientID { get; set; }
        public string? ClinicalNotes { get; set; }
        public string? Diagnosis { get; set; }
        public string? Advice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
