using System;

namespace Pharma_Script.Models
{
    public class Organization
    {
        public int OrganizationID { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public string? OrganizationSlug { get; set; }
        public string OrganizationType { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? AlternatePhone { get; set; }
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public string? GSTNumber { get; set; }
        public string? LicenseNumber { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
