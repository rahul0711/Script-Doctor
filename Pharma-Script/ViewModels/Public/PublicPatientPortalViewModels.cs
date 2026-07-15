using System.Collections.Generic;

namespace Pharma_Script.ViewModels.Public
{
    // One entry in the patient's medical timeline, anchored to a single appointment.
    public class PatientTimelineEntry
    {
        public Pharma_Script.Models.Appointment Appointment { get; set; } = null!;

        // Doctor note — only Diagnosis + Advice are shown to patient (ClinicalNotes is private).
        public Pharma_Script.Models.DoctorNote? Note { get; set; }

        // Prescription header + medicines (null if no prescription was issued)
        public Pharma_Script.Models.Prescription? Prescription { get; set; }
        public List<Pharma_Script.Models.PrescriptionMedicine> Medicines { get; set; } = new();

        // Follow-up scheduled after this appointment (null if none)
        public Pharma_Script.Models.FollowUp? FollowUp { get; set; }
    }

    // Full "My History" view model
    public class PublicPatientHistoryViewModel
    {
        public Pharma_Script.Models.PublicTenant Tenant { get; set; } = null!;
        public Pharma_Script.Models.Patient Patient { get; set; } = null!;
        public List<PatientTimelineEntry> Timeline { get; set; } = new();
        public Pharma_Script.Models.PatientVitals? LatestVitals { get; set; }
    }

    // "My Appointments" view model — paginated list
    public class PublicMyAppointmentsViewModel
    {
        public Pharma_Script.Models.PublicTenant Tenant { get; set; } = null!;
        public Pharma_Script.Models.Patient Patient { get; set; } = null!;
        public IEnumerable<Pharma_Script.Models.Appointment> Appointments { get; set; }
            = new List<Pharma_Script.Models.Appointment>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalItems { get; set; }
        public int PageSize { get; set; } = 10;
        public string? StatusFilter { get; set; }
    }

    // "My Profile" view model
    public class PublicMyProfileViewModel
    {
        public Pharma_Script.Models.PublicTenant Tenant { get; set; } = null!;
        public Pharma_Script.Models.Patient Patient { get; set; } = null!;
        public Pharma_Script.Models.PatientVitals? LatestVitals { get; set; }
    }
}
