using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Pharma_Script.Helpers;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.ViewModels.CMS;
using System.Linq;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers
{
    [Authorize(Roles = "Organization Admin")]
    public class GalleryController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _env;

        public GalleryController(IUnitOfWork uow, IWebHostEnvironment env)
        {
            _uow = uow;
            _env = env;
        }

        private int OrganizationId => User.GetOrganizationId() ?? 0;

        public async Task<IActionResult> Index()
        {
            var items = await _uow.Gallery.GetByOrganizationIdAsync(OrganizationId);
            return View(items);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_Create", new GalleryImageViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GalleryImageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Validation failed.", errors = CollectErrors() });
            }

            if (model.ImageFile == null)
            {
                return Json(new { success = false, message = "Please choose an image to upload." });
            }

            string imagePath;
            try
            {
                imagePath = await CmsImageUploadHelper.UploadAsync(model.ImageFile, _env.WebRootPath, OrganizationId, "gallery");
            }
            catch (CmsUploadValidationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

            var image = new GalleryImage
            {
                OrganizationID = OrganizationId,
                ImageTitle = model.ImageTitle,
                ImagePath = imagePath,
                DisplayOrder = model.DisplayOrder,
                IsActive = model.IsActive
            };

            await _uow.Gallery.AddAsync(image);
            return Json(new { success = true, message = "Image uploaded." });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var image = await _uow.Gallery.GetByIdForOrganizationAsync(id, OrganizationId);
            if (image == null) return NotFound("Image not found.");

            var model = new GalleryImageViewModel
            {
                GalleryID = image.GalleryID,
                ImageTitle = image.ImageTitle,
                DisplayOrder = image.DisplayOrder,
                IsActive = image.IsActive
            };

            ViewBag.CurrentImage = image.ImagePath;
            return PartialView("_Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GalleryImageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Validation failed.", errors = CollectErrors() });
            }

            var image = await _uow.Gallery.GetByIdForOrganizationAsync(model.GalleryID, OrganizationId);
            if (image == null)
            {
                return Json(new { success = false, message = "Image not found." });
            }

            if (model.ImageFile != null)
            {
                try
                {
                    var newImage = await CmsImageUploadHelper.UploadAsync(model.ImageFile, _env.WebRootPath, OrganizationId, "gallery");
                    CmsImageUploadHelper.DeleteIfExists(_env.WebRootPath, image.ImagePath);
                    image.ImagePath = newImage;
                }
                catch (CmsUploadValidationException ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }

            image.ImageTitle = model.ImageTitle;
            image.DisplayOrder = model.DisplayOrder;
            image.IsActive = model.IsActive;

            await _uow.Gallery.UpdateAsync(image);
            return Json(new { success = true, message = "Image updated." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            var result = await _uow.Gallery.SetActiveAsync(id, OrganizationId, isActive);
            if (result)
            {
                return Json(new { success = true, message = $"Image {(isActive ? "activated" : "deactivated")}." });
            }
            return Json(new { success = false, message = "Image not found." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var image = await _uow.Gallery.GetByIdForOrganizationAsync(id, OrganizationId);
            if (image == null)
            {
                return Json(new { success = false, message = "Image not found." });
            }

            var result = await _uow.Gallery.DeleteForOrganizationAsync(id, OrganizationId);
            if (result)
            {
                CmsImageUploadHelper.DeleteIfExists(_env.WebRootPath, image.ImagePath);
                return Json(new { success = true, message = "Image deleted." });
            }
            return Json(new { success = false, message = "Image could not be deleted." });
        }

        private string[] CollectErrors()
        {
            return ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
        }
    }
}
