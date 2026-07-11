using System;

namespace Pharma_Script.Models
{
    public class GalleryImage
    {
        public int GalleryID { get; set; }
        public int OrganizationID { get; set; }
        public string? ImageTitle { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public int DisplayOrder { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
