using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers.Public
{
    // Base for every public tenant-website controller.
    // Resolves the Organization + CMSSettings for the current "{slug}" route value
    // exactly once per request, so individual controllers never look up the
    // organization themselves and never trust an OrganizationID from the client.
    public abstract class PublicControllerBase : Controller
    {
        protected readonly IUnitOfWork Uow;
        protected PublicTenant Tenant { get; private set; } = null!;
        protected int OrganizationId => Tenant.Organization.OrganizationID;

        protected PublicControllerBase(IUnitOfWork uow)
        {
            Uow = uow;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var slug = context.RouteData.Values["slug"] as string;
            var organization = string.IsNullOrWhiteSpace(slug) ? null : await Uow.Organizations.GetBySlugAsync(slug);

            if (organization == null || !organization.IsActive)
            {
                context.Result = View("~/Views/Public/Shared/WebsiteNotAvailable.cshtml");
                return;
            }

            var cmsSettings = await Uow.CMSSettings.GetByOrganizationIdAsync(organization.OrganizationID);
            Tenant = new PublicTenant { Organization = organization, CMSSettings = cmsSettings };
            ViewData["Tenant"] = Tenant;

            await next();
        }
    }
}
