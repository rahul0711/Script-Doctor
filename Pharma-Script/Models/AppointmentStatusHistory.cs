using System;

namespace Pharma_Script.Models
{
    public class AppointmentStatusHistory
    {
        public int HistoryID { get; set; }
        public int AppointmentID { get; set; }
        public string? OldStatus { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public int ChangedByUserID { get; set; }
        public string? Remarks { get; set; }
        public DateTime ChangedAt { get; set; }

        // Joined properties
        public string? ChangedByUserName { get; set; }
    }
}
