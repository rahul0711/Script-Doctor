using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.ViewModels.Public;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers.Public
{
    [AllowAnonymous]
    [Route("{slug:activeOrgSlug}/doctors")]
    public class PublicDoctorsController : PublicControllerBase
    {
        public PublicDoctorsController(IUnitOfWork uow) : base(uow)
        {
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(
            string? searchTerm, int? departmentId, int? branchId, int? specializationId, int page = 1)
        {
            const int pageSize = 9;
            if (page < 1) page = 1;

            var doctors = await Uow.Doctors.SearchAndPaginateAsync(
                OrganizationId, branchId, departmentId, specializationId, true, searchTerm ?? "", page, pageSize);
            var totalItems = await Uow.Doctors.GetSearchCountAsync(
                OrganizationId, branchId, departmentId, specializationId, true, searchTerm ?? "");

            var departments = await Uow.Departments.GetByOrganizationIdAsync(OrganizationId);
            var branches = await Uow.Branches.GetByOrganizationIdAsync(OrganizationId);
            var specializations = await Uow.Specializations.GetAllAsync();

            var model = new PublicDoctorListViewModel
            {
                Tenant = Tenant,
                Doctors = doctors.ToList(),
                Departments = departments.Where(d => d.IsActive).ToList(),
                Branches = branches.Where(b => b.IsActive).ToList(),
                Specializations = specializations.Where(s => s.IsActive).ToList(),
                SearchTerm = searchTerm,
                DepartmentId = departmentId,
                BranchId = branchId,
                SpecializationId = specializationId,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };

            var websiteTitle = Tenant.CMSSettings?.WebsiteTitle ?? Tenant.Organization.OrganizationName;
            ViewData["Title"] = $"Find a Doctor - {websiteTitle}";

            return View(model);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var doctor = await Uow.Doctors.GetDoctorDetailsByIdAsync(id, OrganizationId);
            if (doctor == null || !doctor.IsActive)
            {
                return NotFound();
            }

            var specIds = await Uow.Doctors.GetDoctorSpecializationIDsAsync(id);
            var allSpecs = await Uow.Specializations.GetAllAsync();
            doctor.SpecializationNames = allSpecs.Where(s => specIds.Contains(s.SpecializationID)).Select(s => s.SpecializationName).ToList();

            var availability = await Uow.DoctorAvailabilities.GetAvailabilityByDoctorIdAsync(id);

            var model = new PublicDoctorProfileViewModel
            {
                Tenant = Tenant,
                Doctor = doctor,
                Availability = availability.ToList()
            };

            var websiteTitle = Tenant.CMSSettings?.WebsiteTitle ?? Tenant.Organization.OrganizationName;
            ViewData["Title"] = $"Dr. {doctor.FirstName} {doctor.LastName} - {websiteTitle}";

            return View(model);
        }
    }
}
