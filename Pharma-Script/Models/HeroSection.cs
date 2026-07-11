using System;

namespace Pharma_Script.Models
{
    public class HeroSection
    {
        public int HeroSectionID { get; set; }
        public int OrganizationID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? BannerImage { get; set; }
        public string? ButtonText { get; set; }
        public string? ButtonURL { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
