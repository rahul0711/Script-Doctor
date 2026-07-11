using System;

namespace Pharma_Script.Models
{
    public class DoctorLeave
    {
        public int LeaveID { get; set; }
        public int DoctorID { get; set; }
        public DateTime LeaveStartDate { get; set; }
        public DateTime LeaveEndDate { get; set; }
        public string? Reason { get; set; }

        // Joined fields
        public string? DoctorName { get; set; }
    }
}
