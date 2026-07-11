using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.ViewModels.Specialization;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers
{
    [Authorize(Roles = "Platform Owner,Organization Admin")]
    public class SpecializationsController : Controller
    {
        private readonly IUnitOfWork _uow;

        public SpecializationsController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // GET: /Specializations
        public async Task<IActionResult> Index(string searchTerm = "", int page = 1, int pageSize = 5)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 5;

            var items = await _uow.Specializations.SearchAndPaginateAsync(searchTerm, page, pageSize);
            var totalItems = await _uow.Specializations.GetSearchCountAsync(searchTerm);

            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(items);
        }

        // GET: /Specializations/Details/5 (Partial View Modal)
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var spec = await _uow.Specializations.GetByIdAsync(id);
            if (spec == null)
            {
                return NotFound("Specialization not found.");
            }
            return PartialView("_Details", spec);
        }

        // GET: /Specializations/Create (Partial View Modal)
        [HttpGet]
        public IActionResult Create()
        {
            var model = new SpecializationViewModel();
            return PartialView("_Create", model);
        }

        // POST: /Specializations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SpecializationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Validation failed.", errors });
            }

            try
            {
                // Check uniqueness of Name
                var allSpecs = await _uow.Specializations.GetAllAsync();
                if (allSpecs.Any(s => s.SpecializationName.Equals(model.SpecializationName, StringComparison.OrdinalIgnoreCase)))
                {
                    return Json(new { success = false, message = "A specialization with this name already exists." });
                }

                var spec = new Specialization
                {
                    SpecializationName = model.SpecializationName,
                    Description = model.Description,
                    IsActive = model.IsActive
                };

                await _uow.Specializations.AddAsync(spec);
                return Json(new { success = true, message = "Specialization created successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // GET: /Specializations/Edit/5 (Partial View Modal)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var spec = await _uow.Specializations.GetByIdAsync(id);
            if (spec == null)
            {
                return NotFound("Specialization not found.");
            }

            var model = new SpecializationViewModel
            {
                SpecializationID = spec.SpecializationID,
                SpecializationName = spec.SpecializationName,
                Description = spec.Description,
                IsActive = spec.IsActive
            };

            return PartialView("_Edit", model);
        }

        // POST: /Specializations/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SpecializationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Validation failed.", errors });
            }

            try
            {
                var spec = await _uow.Specializations.GetByIdAsync(model.SpecializationID);
                if (spec == null)
                {
                    return Json(new { success = false, message = "Specialization not found." });
                }

                // Check uniqueness of Name if changed
                if (!spec.SpecializationName.Equals(model.SpecializationName, StringComparison.OrdinalIgnoreCase))
                {
                    var allSpecs = await _uow.Specializations.GetAllAsync();
                    if (allSpecs.Any(s => s.SpecializationName.Equals(model.SpecializationName, StringComparison.OrdinalIgnoreCase)))
                    {
                        return Json(new { success = false, message = "A specialization with this name already exists." });
                    }
                }

                spec.SpecializationName = model.SpecializationName;
                spec.Description = model.Description;
                spec.IsActive = model.IsActive;

                await _uow.Specializations.UpdateAsync(spec);
                return Json(new { success = true, message = "Specialization updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // POST: /Specializations/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _uow.Specializations.DeleteAsync(id);
                if (result)
                {
                    return Json(new { success = true, message = "Specialization deleted successfully." });
                }
                return Json(new { success = false, message = "Specialization could not be found or deleted." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
