using System;
using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.Doctor
{
    public class DoctorAvailabilityViewModel
    {
        public int AvailabilityID { get; set; }

        [Required(ErrorMessage = "Doctor is required.")]
        public int DoctorID { get; set; }

        [Required(ErrorMessage = "Day of week is required.")]
        [Display(Name = "Day of Week")]
        public string DayOfWeek { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start Time is required.")]
        [Display(Name = "Start Time")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End Time is required.")]
        [Display(Name = "End Time")]
        public TimeSpan EndTime { get; set; }

        [Required(ErrorMessage = "Slot duration is required.")]
        [Range(5, 180, ErrorMessage = "Slot duration must be between 5 and 180 minutes.")]
        [Display(Name = "Slot Duration (Minutes)")]
        public int SlotDuration { get; set; } = 15;

        [Display(Name = "Break Start Time")]
        public TimeSpan? BreakStart { get; set; }

        [Display(Name = "Break End Time")]
        public TimeSpan? BreakEnd { get; set; }

        [Display(Name = "Is Available?")]
        public bool IsAvailable { get; set; } = true;
    }
}
