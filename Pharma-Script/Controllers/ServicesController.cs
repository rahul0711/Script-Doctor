using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Pharma_Script.Helpers;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.ViewModels.CMS;
using System.Linq;
using System.Threading.Tasks;
using ServiceModel = Pharma_Script.Models.Service;

namespace Pharma_Script.Controllers
{
    [Authorize(Roles = "Organization Admin")]
    public class ServicesController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _env;

        public ServicesController(IUnitOfWork uow, IWebHostEnvironment env)
        {
            _uow = uow;
            _env = env;
        }

        private int OrganizationId => User.GetOrganizationId() ?? 0;

        public async Task<IActionResult> Index()
        {
            var items = await _uow.Services.GetByOrganizationIdAsync(OrganizationId);
            return View(items);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_Create", new ServiceViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Validation failed.", errors = CollectErrors() });
            }

            var service = new ServiceModel
            {
                OrganizationID = OrganizationId,
                ServiceName = model.ServiceName,
                Description = model.Description,
                IsActive = model.IsActive
            };

            if (model.ServiceImage != null)
            {
                try
                {
                    service.ServiceImage = await CmsImageUploadHelper.UploadAsync(model.ServiceImage, _env.WebRootPath, OrganizationId, "services");
                }
                catch (CmsUploadValidationException ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }

            await _uow.Services.AddAsync(service);
            return Json(new { success = true, message = "Service created." });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var service = await _uow.Services.GetByIdForOrganizationAsync(id, OrganizationId);
            if (service == null) return NotFound("Service not found.");

            var model = new ServiceViewModel
            {
                ServiceID = service.ServiceID,
                ServiceName = service.ServiceName,
                Description = service.Description,
                ExistingServiceImage = service.ServiceImage,
                IsActive = service.IsActive
            };

            return PartialView("_Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ServiceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Validation failed.", errors = CollectErrors() });
            }

            var service = await _uow.Services.GetByIdForOrganizationAsync(model.ServiceID, OrganizationId);
            if (service == null)
            {
                return Json(new { success = false, message = "Service not found." });
            }

            if (model.ServiceImage != null)
            {
                try
                {
                    var newImage = await CmsImageUploadHelper.UploadAsync(model.ServiceImage, _env.WebRootPath, OrganizationId, "services");
                    CmsImageUploadHelper.DeleteIfExists(_env.WebRootPath, service.ServiceImage);
                    service.ServiceImage = newImage;
                }
                catch (CmsUploadValidationException ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }

            service.ServiceName = model.ServiceName;
            service.Description = model.Description;
            service.IsActive = model.IsActive;

            await _uow.Services.UpdateAsync(service);
            return Json(new { success = true, message = "Service updated." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            var result = await _uow.Services.SetActiveAsync(id, OrganizationId, isActive);
            if (result)
            {
                return Json(new { success = true, message = $"Service {(isActive ? "activated" : "deactivated")}." });
            }
            return Json(new { success = false, message = "Service not found." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _uow.Services.GetByIdForOrganizationAsync(id, OrganizationId);
            if (service == null)
            {
                return Json(new { success = false, message = "Service not found." });
            }

            var result = await _uow.Services.DeleteForOrganizationAsync(id, OrganizationId);
            if (result)
            {
                CmsImageUploadHelper.DeleteIfExists(_env.WebRootPath, service.ServiceImage);
                return Json(new { success = true, message = "Service deleted." });
            }
            return Json(new { success = false, message = "Service could not be deleted." });
        }

        private string[] CollectErrors()
        {
            return ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
        }
    }
}
