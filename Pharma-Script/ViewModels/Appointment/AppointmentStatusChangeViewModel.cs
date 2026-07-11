using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.Appointment
{
    public class AppointmentStatusChangeViewModel
    {
        [Required]
        public int AppointmentID { get; set; }

        [Required]
        public string NewStatus { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? Remarks { get; set; }
    }
}
