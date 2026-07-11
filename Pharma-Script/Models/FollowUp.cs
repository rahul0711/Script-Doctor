using System;

namespace Pharma_Script.Models
{
    public class FollowUp
    {
        public int FollowUpID { get; set; }
        public int AppointmentID { get; set; }
        public int OrganizationID { get; set; }
        public int DoctorID { get; set; }
        public int PatientID { get; set; }
        public DateTime FollowUpDate { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Completed, Cancelled
        public DateTime CreatedAt { get; set; }

        // Joined properties
        public string? DoctorName { get; set; }
        public string? PatientName { get; set; }
    }
}
