using System;

namespace Pharma_Script.Models
{
    public class Service
    {
        public int ServiceID { get; set; }
        public int OrganizationID { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ServiceImage { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
