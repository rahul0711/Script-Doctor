using System;
using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.Appointment
{
    public class AppointmentRescheduleViewModel
    {
        [Required]
        public int AppointmentID { get; set; }

        [Required(ErrorMessage = "Reschedule date is required.")]
        [DataType(DataType.Date)]
        public DateTime NewDate { get; set; }

        [Required(ErrorMessage = "Reschedule time slot is required.")]
        public string NewSlot { get; set; } = string.Empty; // e.g. "14:15"

        [Required(ErrorMessage = "Please state the reason for rescheduling.")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
        public string Remarks { get; set; } = string.Empty;
    }
}
