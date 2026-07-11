using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharma_Script.Helpers;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.Services.Interfaces;
using Pharma_Script.ViewModels.Organization;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers
{
    [Authorize(Roles = "Platform Owner")]
    public class OrganizationsController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IOrganizationSlugCache _slugCache;

        public OrganizationsController(IUnitOfWork uow, IOrganizationSlugCache slugCache)
        {
            _uow = uow;
            _slugCache = slugCache;
        }

        private static string GenerateSlug(string value) => SlugHelper.GenerateSlug(value);

        // GET: /Organizations
        public async Task<IActionResult> Index(string searchTerm = "", int page = 1, int pageSize = 5)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 5;

            var items = await _uow.Organizations.SearchAndPaginateAsync(searchTerm, page, pageSize);
            var totalItems = await _uow.Organizations.GetSearchCountAsync(searchTerm);

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(items);
        }

        // GET: /Organizations/Details/5 (Partial View for Modal)
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var org = await _uow.Organizations.GetByIdAsync(id);
            if (org == null)
            {
                return NotFound("Organization not found.");
            }
            return PartialView("_Details", org);
        }

        // GET: /Organizations/Create (Partial View for Modal)
        [HttpGet]
        public IActionResult Create()
        {
            var model = new OrganizationViewModel();
            return PartialView("_Create", model);
        }

        // POST: /Organizations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrganizationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Validation failed.", errors });
            }

            try
            {
                var slug = string.IsNullOrWhiteSpace(model.OrganizationSlug)
                    ? GenerateSlug(model.OrganizationName)
                    : GenerateSlug(model.OrganizationSlug);

                if (await _uow.Organizations.IsSlugTakenAsync(slug, null))
                {
                    return Json(new { success = false, message = $"Website slug '{slug}' is already in use by another organization." });
                }

                var org = new Organization
                {
                    OrganizationName = model.OrganizationName,
                    OrganizationSlug = slug,
                    OrganizationType = model.OrganizationType,
                    Email = model.Email,
                    Phone = model.Phone,
                    AlternatePhone = model.AlternatePhone,
                    AddressLine1 = model.AddressLine1,
                    AddressLine2 = model.AddressLine2,
                    City = model.City,
                    State = model.State,
                    Country = model.Country,
                    Pincode = model.Pincode,
                    GSTNumber = model.GSTNumber,
                    LicenseNumber = model.LicenseNumber,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.Now
                };

                await _uow.Organizations.AddAsync(org);
                await _slugCache.RefreshAsync(_uow);
                return Json(new { success = true, message = "Organization created successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // GET: /Organizations/Edit/5 (Partial View for Modal)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var org = await _uow.Organizations.GetByIdAsync(id);
            if (org == null)
            {
                return NotFound("Organization not found.");
            }

            var model = new OrganizationViewModel
            {
                OrganizationID = org.OrganizationID,
                OrganizationName = org.OrganizationName,
                OrganizationSlug = org.OrganizationSlug,
                OrganizationType = org.OrganizationType,
                Email = org.Email,
                Phone = org.Phone,
                AlternatePhone = org.AlternatePhone,
                AddressLine1 = org.AddressLine1,
                AddressLine2 = org.AddressLine2,
                City = org.City,
                State = org.State,
                Country = org.Country,
                Pincode = org.Pincode,
                GSTNumber = org.GSTNumber,
                LicenseNumber = org.LicenseNumber,
                IsActive = org.IsActive
            };

            return PartialView("_Edit", model);
        }

        // POST: /Organizations/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OrganizationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Validation failed.", errors });
            }

            try
            {
                var org = await _uow.Organizations.GetByIdAsync(model.OrganizationID);
                if (org == null)
                {
                    return Json(new { success = false, message = "Organization not found." });
                }

                var slug = string.IsNullOrWhiteSpace(model.OrganizationSlug)
                    ? GenerateSlug(model.OrganizationName)
                    : GenerateSlug(model.OrganizationSlug);

                if (await _uow.Organizations.IsSlugTakenAsync(slug, org.OrganizationID))
                {
                    return Json(new { success = false, message = $"Website slug '{slug}' is already in use by another organization." });
                }

                org.OrganizationName = model.OrganizationName;
                org.OrganizationSlug = slug;
                org.OrganizationType = model.OrganizationType;
                org.Email = model.Email;
                org.Phone = model.Phone;
                org.AlternatePhone = model.AlternatePhone;
                org.AddressLine1 = model.AddressLine1;
                org.AddressLine2 = model.AddressLine2;
                org.City = model.City;
                org.State = model.State;
                org.Country = model.Country;
                org.Pincode = model.Pincode;
                org.GSTNumber = model.GSTNumber;
                org.LicenseNumber = model.LicenseNumber;
                org.IsActive = model.IsActive;
                org.UpdatedAt = DateTime.Now;

                await _uow.Organizations.UpdateAsync(org);
                await _slugCache.RefreshAsync(_uow);
                return Json(new { success = true, message = "Organization updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // POST: /Organizations/ToggleStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            try
            {
                var result = await _uow.Organizations.UpdateStatusAsync(id, isActive);
                if (result)
                {
                    await _slugCache.RefreshAsync(_uow);
                    return Json(new { success = true, message = $"Organization {(isActive ? "activated" : "deactivated")} successfully." });
                }
                return Json(new { success = false, message = "Organization not found or status could not be changed." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // POST: /Organizations/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _uow.Organizations.DeleteAsync(id);
                if (result)
                {
                    await _slugCache.RefreshAsync(_uow);
                    return Json(new { success = true, message = "Organization deleted successfully." });
                }
                return Json(new { success = false, message = "Organization not found or could not be deleted." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
