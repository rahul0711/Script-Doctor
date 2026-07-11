using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pharma_Script.Helpers;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.ViewModels.Department;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers
{
    [Authorize(Roles = "Platform Owner,Organization Admin")]
    public class DepartmentsController : Controller
    {
        private readonly IUnitOfWork _uow;

        public DepartmentsController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // GET: /Departments
        public async Task<IActionResult> Index()
        {
            var isPlatformOwner = User.IsPlatformOwner();
            var orgId = User.GetOrganizationId();

            IEnumerable<Department> departments;

            if (isPlatformOwner)
            {
                departments = await _uow.Departments.GetAllAsync();
            }
            else if (orgId.HasValue)
            {
                departments = await _uow.Departments.GetByOrganizationIdAsync(orgId.Value);
            }
            else
            {
                departments = new List<Department>();
            }

            return View(departments);
        }

        // GET: /Departments/Details/5 (Partial View for Modal)
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var department = await _uow.Departments.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound("Department not found.");
            }

            // Tenant Isolation check
            var userOrgId = User.GetOrganizationId();
            if (userOrgId.HasValue && department.OrganizationID != userOrgId.Value)
            {
                return Forbid();
            }

            return PartialView("_Details", department);
        }

        // GET: /Departments/Create (Partial View for Modal)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new DepartmentViewModel();
            var isPlatformOwner = User.IsPlatformOwner();
            var userOrgId = User.GetOrganizationId();

            if (isPlatformOwner)
            {
                var orgs = await _uow.Organizations.GetAllAsync();
                ViewBag.Organizations = new SelectList(orgs.Where(o => o.IsActive), "OrganizationID", "OrganizationName");
            }
            else if (userOrgId.HasValue)
            {
                model.OrganizationID = userOrgId.Value;
            }

            return PartialView("_Create", model);
        }

        // POST: /Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentViewModel model)
        {
            var userOrgId = User.GetOrganizationId();
            if (userOrgId.HasValue)
            {
                model.OrganizationID = userOrgId.Value;
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Validation failed.", errors });
            }

            try
            {
                var dept = new Department
                {
                    OrganizationID = model.OrganizationID,
                    DepartmentName = model.DepartmentName,
                    Description = model.Description,
                    IsActive = model.IsActive
                };

                await _uow.Departments.AddAsync(dept);
                return Json(new { success = true, message = "Department created successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // GET: /Departments/Edit/5 (Partial View for Modal)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var department = await _uow.Departments.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound("Department not found.");
            }

            // Tenant Isolation check
            var userOrgId = User.GetOrganizationId();
            if (userOrgId.HasValue && department.OrganizationID != userOrgId.Value)
            {
                return Forbid();
            }

            var model = new DepartmentViewModel
            {
                DepartmentID = department.DepartmentID,
                OrganizationID = department.OrganizationID,
                DepartmentName = department.DepartmentName,
                Description = department.Description,
                IsActive = department.IsActive
            };

            if (User.IsPlatformOwner())
            {
                var orgs = await _uow.Organizations.GetAllAsync();
                ViewBag.Organizations = new SelectList(orgs.Where(o => o.IsActive), "OrganizationID", "OrganizationName", department.OrganizationID);
            }

            return PartialView("_Edit", model);
        }

        // POST: /Departments/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DepartmentViewModel model)
        {
            var userOrgId = User.GetOrganizationId();
            if (userOrgId.HasValue)
            {
                model.OrganizationID = userOrgId.Value;
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Validation failed.", errors });
            }

            try
            {
                var dept = await _uow.Departments.GetByIdAsync(model.DepartmentID);
                if (dept == null)
                {
                    return Json(new { success = false, message = "Department not found." });
                }

                // Verify tenant boundaries
                if (userOrgId.HasValue && dept.OrganizationID != userOrgId.Value)
                {
                    return Json(new { success = false, message = "Unauthorized access." });
                }

                dept.OrganizationID = model.OrganizationID;
                dept.DepartmentName = model.DepartmentName;
                dept.Description = model.Description;
                dept.IsActive = model.IsActive;

                await _uow.Departments.UpdateAsync(dept);
                return Json(new { success = true, message = "Department updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // POST: /Departments/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var dept = await _uow.Departments.GetByIdAsync(id);
                if (dept == null)
                {
                    return Json(new { success = false, message = "Department not found." });
                }

                // Tenant Isolation check
                var userOrgId = User.GetOrganizationId();
                if (userOrgId.HasValue && dept.OrganizationID != userOrgId.Value)
                {
                    return Json(new { success = false, message = "Unauthorized access." });
                }

                var result = await _uow.Departments.DeleteAsync(id);
                if (result)
                {
                    return Json(new { success = true, message = "Department deleted successfully." });
                }
                return Json(new { success = false, message = "Department could not be deleted." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
