using System;

namespace Pharma_Script.Models
{
    public class User
    {
        public int UserID { get; set; }
        public int? OrganizationID { get; set; }
        public int RoleID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? ProfileImage { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigational fields
        public string? RoleName { get; set; }
        public string? OrganizationName { get; set; }
    }
}
