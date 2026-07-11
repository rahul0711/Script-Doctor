using System;

namespace Pharma_Script.Models
{
    public class ContactMessage
    {
        public int ContactMessageID { get; set; }
        public int OrganizationID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Subject { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
