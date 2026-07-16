using System;

namespace Pharma_Script.Models
{
    public class Appointment
    {
        public int AppointmentID { get; set; }
        public int OrganizationID { get; set; }
        public int? BranchID { get; set; }
        public int DoctorID { get; set; }
        public int PatientID { get; set; }
        public string AppointmentType { get; set; } = "Video"; // Video, Voice
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal ConsultationFee { get; set; }
        public bool PriorityConsultation { get; set; }
        public string? Symptoms { get; set; }
        public string? AppointmentReason { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Rescheduled, Completed, Cancelled, No Show
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Joined properties
        public string? DoctorName { get; set; }
        public string? DoctorEmail { get; set; }
        public string? DoctorPhone { get; set; }
        public string? DoctorSpecializations { get; set; }
        public string? PatientName { get; set; }
        public string? PatientGender { get; set; }
        public string? PatientBloodGroup { get; set; }
        public DateTime? PatientDOB { get; set; }
        public string? BranchName { get; set; }
        public string? OrganizationName { get; set; }
        public string? PaymentStatus { get; set; } // Pending, Paid, Failed, Refunded - from most recent linked Payment, if any
    }
}
