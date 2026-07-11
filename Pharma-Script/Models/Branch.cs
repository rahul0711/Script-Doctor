using System;

namespace Pharma_Script.Models
{
    public class Branch
    {
        public int BranchID { get; set; }
        public int OrganizationID { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public bool IsMainBranch { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation reference representation
        public string? OrganizationName { get; set; }
    }
}
