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
    public class WebsiteSettingsController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _env;

        public WebsiteSettingsController(IUnitOfWork uow, IWebHostEnvironment env)
        {
            _uow = uow;
            _env = env;
        }

        private int OrganizationId => User.GetOrganizationId() ?? 0;

        private async Task<CMSSetting> GetOrCreateSettingsAsync()
        {
            var settings = await _uow.CMSSettings.GetByOrganizationIdAsync(OrganizationId);
            if (settings != null) return settings;

            var org = await _uow.Organizations.GetByIdAsync(OrganizationId);
            return new CMSSetting
            {
                OrganizationID = OrganizationId,
                WebsiteTitle = org?.OrganizationName ?? "My Clinic"
            };
        }

        public async Task<IActionResult> Index()
        {
            var settings = await GetOrCreateSettingsAsync();
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGeneral(WebsiteGeneralViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Validation failed.", errors = CollectErrors() });
            }

            var settings = await GetOrCreateSettingsAsync();
            settings.WebsiteTitle = model.WebsiteTitle;
            settings.UpdatedAt = DateTime.Now;
            await _uow.CMSSettings.UpsertAsync(settings);

            return Json(new { success = true, message = "General settings saved." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBranding(WebsiteBrandingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Validation failed.", errors = CollectErrors() });
            }

            var settings = await GetOrCreateSettingsAsync();

            try
            {
                if (model.Logo != null)
                {
                    var newLogo = await CmsImageUploadHelper.UploadAsync(model.Logo, _env.WebRootPath, OrganizationId, "branding", 2 * 1024 * 1024);
                    CmsImageUploadHelper.DeleteIfExists(_env.WebRootPath, settings.WebsiteLogo);
                    settings.WebsiteLogo = newLogo;
                }

                if (model.Favicon != null)
                {
                    var newFavicon = await CmsImageUploadHelper.UploadAsync(model.Favicon, _env.WebRootPath, OrganizationId, "branding", 512 * 1024, new[] { ".png", ".ico" });
                    CmsImageUploadHelper.DeleteIfExists(_env.WebRootPath, settings.Favicon);
                    settings.Favicon = newFavicon;
                }
            }
            catch (CmsUploadValidationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

            settings.PrimaryColor = model.PrimaryColor;
            settings.SecondaryColor = model.SecondaryColor;
            settings.UpdatedAt = DateTime.Now;
            await _uow.CMSSettings.UpsertAsync(settings);

            return Json(new { success = true, message = "Branding saved." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAbout(WebsiteAboutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Validation failed.", errors = CollectErrors() });
            }

            var settings = await GetOrCreateSettingsAsync();
            settings.AboutUs = model.AboutUs;
            settings.Mission = model.Mission;
            settings.Vision = model.Vision;
            settings.UpdatedAt = DateTime.Now;
            await _uow.CMSSettings.UpsertAsync(settings);

            return Json(new { success = true, message = "About section saved." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveContact(WebsiteContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Validation failed.", errors = CollectErrors() });
            }

            string? safeMapUrl = null;
            if (!string.IsNullOrWhiteSpace(model.GoogleMapEmbed))
            {
                if (!GoogleMapEmbedHelper.TryGetSafeEmbedUrl(model.GoogleMapEmbed, out safeMapUrl))
                {
                    return Json(new { success = false, message = "The Google Map embed must be a valid Google Maps URL or embed snippet." });
                }
            }

            var settings = await GetOrCreateSettingsAsync();
            settings.ContactEmail = model.ContactEmail;
            settings.ContactPhone = model.ContactPhone;
            settings.EmergencyPhone = model.EmergencyPhone;
            settings.Address = model.Address;
            settings.GoogleMapEmbed = safeMapUrl;
            settings.UpdatedAt = DateTime.Now;
            await _uow.CMSSettings.UpsertAsync(settings);

            return Json(new { success = true, message = "Contact information saved." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSocial(WebsiteSocialViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Validation failed.", errors = CollectErrors() });
            }

            var settings = await GetOrCreateSettingsAsync();
            settings.FacebookURL = model.FacebookURL;
            settings.InstagramURL = model.InstagramURL;
            settings.LinkedInURL = model.LinkedInURL;
            settings.TwitterURL = model.TwitterURL;
            settings.YouTubeURL = model.YouTubeURL;
            settings.UpdatedAt = DateTime.Now;
            await _uow.CMSSettings.UpsertAsync(settings);

            return Json(new { success = true, message = "Social media links saved." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFooter(WebsiteFooterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Validation failed.", errors = CollectErrors() });
            }

            var settings = await GetOrCreateSettingsAsync();
            settings.FooterText = model.FooterText;
            settings.UpdatedAt = DateTime.Now;
            await _uow.CMSSettings.UpsertAsync(settings);

            return Json(new { success = true, message = "Footer saved." });
        }

        private string[] CollectErrors()
        {
            return ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
        }
    }
}
