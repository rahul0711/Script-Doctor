using Pharma_Script.Models;
using System.Collections.Generic;

namespace Pharma_Script.ViewModels.Public
{
    public class PublicHomeViewModel
    {
        public PublicTenant Tenant { get; set; } = null!;
        public List<HeroSection> HeroSections { get; set; } = new();
        public List<Pharma_Script.Models.Department> Departments { get; set; } = new();
        public List<Pharma_Script.Models.Doctor> FeaturedDoctors { get; set; } = new();
        public List<Service> Services { get; set; } = new();
        public List<GalleryImage> GalleryPreview { get; set; } = new();
        public List<FAQ> FAQs { get; set; } = new();
        public int TotalDoctorCount { get; set; }
    }
}
