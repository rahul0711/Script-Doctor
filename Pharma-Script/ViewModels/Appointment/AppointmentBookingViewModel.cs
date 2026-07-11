using System;
using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.Appointment
{
    public class AppointmentBookingViewModel
    {
        [Required(ErrorMessage = "Doctor selection is required.")]
        public int DoctorID { get; set; }

        [Required(ErrorMessage = "Consultation type is required.")]
        public string AppointmentType { get; set; } = "Clinic"; // Clinic, Video, Voice

        public bool PriorityConsultation { get; set; }

        [Required(ErrorMessage = "Appointment date is required.")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Time slot selection is required.")]
        public string SelectedSlot { get; set; } = string.Empty; // e.g. "09:00"

        [StringLength(1000, ErrorMessage = "Symptoms description cannot exceed 1000 characters.")]
        public string? Symptoms { get; set; }

        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
        public string? AppointmentReason { get; set; }

        public int PatientID { get; set; }
    }
}
