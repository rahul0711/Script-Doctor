using System;
using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.ViewModels.Doctor
{
    public class DoctorLeaveViewModel
    {
        public int LeaveID { get; set; }

        [Required(ErrorMessage = "Doctor is required.")]
        public int DoctorID { get; set; }

        [Required(ErrorMessage = "Leave Start Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Leave Start Date")]
        public DateTime LeaveStartDate { get; set; }

        [Required(ErrorMessage = "Leave End Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Leave End Date")]
        public DateTime LeaveEndDate { get; set; }

        [StringLength(250, ErrorMessage = "Reason cannot exceed 250 characters.")]
        public string? Reason { get; set; }
    }
}
