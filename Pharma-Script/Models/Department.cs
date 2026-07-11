using System;

namespace Pharma_Script.Models
{
    public class Department
    {
        public int DepartmentID { get; set; }
        public int OrganizationID { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation reference representation
        public string? OrganizationName { get; set; }
    }
}
