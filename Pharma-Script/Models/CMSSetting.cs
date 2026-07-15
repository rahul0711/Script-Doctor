using System;

namespace Pharma_Script.Models
{
    public class CMSSetting
    {
        public int CMSSettingID { get; set; }
        public int OrganizationID { get; set; }
        public string WebsiteTitle { get; set; } = string.Empty;
        public string? WebsiteLogo { get; set; }
        public string? Favicon { get; set; }
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public string? AboutUs { get; set; }
        public string? Mission { get; set; }
        public string? Vision { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? EmergencyPhone { get; set; }
        public string? Address { get; set; }
        public string? GoogleMapEmbed { get; set; }
        public string? FooterText { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
