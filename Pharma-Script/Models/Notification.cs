using System;

namespace Pharma_Script.Models
{
    public class Notification
    {
        public int NotificationID { get; set; }
        public int OrganizationID { get; set; }
        public int UserID { get; set; }
        
        public string NotificationType { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityID { get; set; }
        
        public bool IsRead { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
