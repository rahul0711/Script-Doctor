using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.ViewModels.Public;
using System.Linq;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers.Public
{
    [AllowAnonymous]
    [Route("{slug:activeOrgSlug}")]
    public class PublicHomeController : PublicControllerBase
    {
        public PublicHomeController(IUnitOfWork uow) : base(uow)
        {
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var heroSections = await Uow.HeroSections.GetActiveByOrganizationIdAsync(OrganizationId);
            var departments = await Uow.Departments.GetByOrganizationIdAsync(OrganizationId);
            var featuredDoctors = await Uow.Doctors.SearchAndPaginateAsync(OrganizationId, null, null, null, true, "", 1, 6);
            var totalDoctorCount = await Uow.Doctors.GetSearchCountAsync(OrganizationId, null, null, null, true, "");
            var services = await Uow.Services.GetActiveByOrganizationIdAsync(OrganizationId);

            var model = new PublicHomeViewModel
            {
                Tenant = Tenant,
                HeroSections = heroSections.ToList(),
                Departments = departments.Where(d => d.IsActive).ToList(),
                FeaturedDoctors = featuredDoctors.ToList(),
                TotalDoctorCount = totalDoctorCount,
                Services = services.ToList()
            };

            var websiteTitle = Tenant.CMSSettings?.WebsiteTitle ?? Tenant.Organization.OrganizationName;
            ViewData["Title"] = websiteTitle;
            ViewData["MetaDescription"] = Tenant.CMSSettings?.AboutUs?.Length > 160
                ? Tenant.CMSSettings.AboutUs.Substring(0, 160)
                : Tenant.CMSSettings?.AboutUs;

            return View(model);
        }
    }
}
