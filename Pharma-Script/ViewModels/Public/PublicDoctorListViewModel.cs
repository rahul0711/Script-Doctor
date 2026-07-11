using Pharma_Script.Models;
using System.Collections.Generic;

namespace Pharma_Script.ViewModels.Public
{
    public class PublicDoctorListViewModel
    {
        public PublicTenant Tenant { get; set; } = null!;
        public List<Pharma_Script.Models.Doctor> Doctors { get; set; } = new();
        public List<Pharma_Script.Models.Department> Departments { get; set; } = new();
        public List<Pharma_Script.Models.Branch> Branches { get; set; } = new();
        public List<Pharma_Script.Models.Specialization> Specializations { get; set; } = new();

        public string? SearchTerm { get; set; }
        public int? DepartmentId { get; set; }
        public int? BranchId { get; set; }
        public int? SpecializationId { get; set; }

        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 9;
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}
