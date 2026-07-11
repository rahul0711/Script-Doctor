using System;

namespace Pharma_Script.Models
{
    public class Receptionist
    {
        public int ReceptionistID { get; set; }
        public int UserID { get; set; }
        public int OrganizationID { get; set; }
        public int? BranchID { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Joined fields for display
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? BranchName { get; set; }
        public string? OrganizationName { get; set; }
    }
}
