using System;

namespace Pharma_Script.Models
{
    public class DoctorAvailability
    {
        public int AvailabilityID { get; set; }
        public int DoctorID { get; set; }
        public string DayOfWeek { get; set; } = string.Empty; // Monday, Tuesday, etc.
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int SlotDuration { get; set; } = 15;
        public TimeSpan? BreakStart { get; set; }
        public TimeSpan? BreakEnd { get; set; }
        public bool IsAvailable { get; set; } = true;

        // Joined fields
        public string? DoctorName { get; set; }
    }
}
