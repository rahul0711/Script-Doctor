using System;
using System.ComponentModel.DataAnnotations;

namespace Pharma_Script.Models
{
    public class ConsultationSession
    {
        public int ConsultationSessionID { get; set; }
        
        [Required]
        public int OrganizationID { get; set; }
        
        [Required]
        public int AppointmentID { get; set; }
        
        [Required]
        public int DoctorID { get; set; }
        
        [Required]
        public int PatientID { get; set; }
        
        [Required]
        [StringLength(20)]
        public string ConsultationType { get; set; } = "Video"; // Video or Voice
        
        [StringLength(50)]
        public string? MeetingProvider { get; set; }
        
        [StringLength(500)]
        public string? MeetingURL { get; set; }
        
        [Required]
        [StringLength(20)]
        public string SessionStatus { get; set; } = "Pending";
        
        public int CreatedByUserID { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
