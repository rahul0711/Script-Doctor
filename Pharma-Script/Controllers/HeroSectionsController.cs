using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Pharma_Script.Helpers;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.ViewModels.CMS;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers
{
    [Authorize(Roles = "Organization Admin")]
    public class HeroSectionsController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _env;

        public HeroSectionsController(IUnitOfWork uow, IWebHostEnvironment env)
        {
            _uow = uow;
            _env = env;
        }

        private int OrganizationId => User.GetOrganizationId() ?? 0;

        public async Task<IActionResult> Index()
        {
            var items = await _uow.HeroSections.GetByOrganizationIdAsync(OrganizationId);
            return View(items);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_Create", new HeroSectionViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HeroSectionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Validation failed.", errors = CollectErrors() });
            }

            var hero = new HeroSection
            {
                OrganizationID = OrganizationId,
                Title = model.Title,
                Subtitle = model.Subtitle,
                ButtonText = model.ButtonText,
                ButtonURL = model.ButtonURL,
                IsActive = model.IsActive
            };

            if (model.BannerImage != null)
            {
                try
                {
                    hero.BannerImage = await CmsImageUploadHelper.UploadAsync(model.BannerImage, _env.WebRootPath, OrganizationId, "hero");
                }
                catch (CmsUploadValidationException ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }

            await _uow.HeroSections.AddAsync(hero);
            return Json(new { success = true, message = "Hero section created." });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var hero = await _uow.HeroSections.GetByIdForOrganizationAsync(id, OrganizationId);
            if (hero == null) return NotFound("Hero section not found.");

            var model = new HeroSectionViewModel
            {
                HeroSectionID = hero.HeroSectionID,
                Title = hero.Title,
                Subtitle = hero.Subtitle,
                ExistingBannerImage = hero.BannerImage,
                ButtonText = hero.ButtonText,
                ButtonURL = hero.ButtonURL,
                IsActive = hero.IsActive
            };

            return PartialView("_Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(HeroSectionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Validation failed.", errors = CollectErrors() });
            }

            var hero = await _uow.HeroSections.GetByIdForOrganizationAsync(model.HeroSectionID, OrganizationId);
            if (hero == null)
            {
                return Json(new { success = false, message = "Hero section not found." });
            }

            if (model.BannerImage != null)
            {
                try
                {
                    var newImage = await CmsImageUploadHelper.UploadAsync(model.BannerImage, _env.WebRootPath, OrganizationId, "hero");
                    CmsImageUploadHelper.DeleteIfExists(_env.WebRootPath, hero.BannerImage);
                    hero.BannerImage = newImage;
                }
                catch (CmsUploadValidationException ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }

            hero.Title = model.Title;
            hero.Subtitle = model.Subtitle;
            hero.ButtonText = model.ButtonText;
            hero.ButtonURL = model.ButtonURL;
            hero.IsActive = model.IsActive;

            await _uow.HeroSections.UpdateAsync(hero);
            return Json(new { success = true, message = "Hero section updated." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            var result = await _uow.HeroSections.SetActiveAsync(id, OrganizationId, isActive);
            if (result)
            {
                return Json(new { success = true, message = $"Hero section {(isActive ? "activated" : "deactivated")}." });
            }
            return Json(new { success = false, message = "Hero section not found." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var hero = await _uow.HeroSections.GetByIdForOrganizationAsync(id, OrganizationId);
            if (hero == null)
            {
                return Json(new { success = false, message = "Hero section not found." });
            }

            var result = await _uow.HeroSections.DeleteForOrganizationAsync(id, OrganizationId);
            if (result)
            {
                CmsImageUploadHelper.DeleteIfExists(_env.WebRootPath, hero.BannerImage);
                return Json(new { success = true, message = "Hero section deleted." });
            }
            return Json(new { success = false, message = "Hero section could not be deleted." });
        }

        private string[] CollectErrors()
        {
            return ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
        }
    }
}
