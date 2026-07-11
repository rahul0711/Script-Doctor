using System;

namespace Pharma_Script.Models
{
    public class FAQ
    {
        public int FAQID { get; set; }
        public int OrganizationID { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public int DisplayOrder { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
