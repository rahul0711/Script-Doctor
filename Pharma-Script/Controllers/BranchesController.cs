using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pharma_Script.Helpers;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.ViewModels.Branch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers
{
    [Authorize(Roles = "Platform Owner,Organization Admin")]
    public class BranchesController : Controller
    {
        private readonly IUnitOfWork _uow;

        public BranchesController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // GET: /Branches
        public async Task<IActionResult> Index()
        {
            var isPlatformOwner = User.IsPlatformOwner();
            var orgId = User.GetOrganizationId();

            IEnumerable<Branch> branches;

            if (isPlatformOwner)
            {
                // Platform Owner sees all branches
                branches = await _uow.Branches.GetAllAsync();
            }
            else if (orgId.HasValue)
            {
                // Admin sees only their organization's branches
                branches = await _uow.Branches.GetByOrganizationIdAsync(orgId.Value);
            }
            else
            {
                branches = new List<Branch>();
            }

            return View(branches);
        }

        // GET: /Branches/Details/5 (Partial View for Modal)
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var branch = await _uow.Branches.GetByIdAsync(id);
            if (branch == null)
            {
                return NotFound("Branch not found.");
            }

            // Tenant Isolation check
            var userOrgId = User.GetOrganizationId();
            if (userOrgId.HasValue && branch.OrganizationID != userOrgId.Value)
            {
                return Forbid();
            }

            return PartialView("_Details", branch);
        }

        // GET: /Branches/Create (Partial View for Modal)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new BranchViewModel();
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

        // POST: /Branches/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BranchViewModel model)
        {
            // Security verification: If user is organization admin, override any spoofed OrganizationID
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
                var branch = new Branch
                {
                    OrganizationID = model.OrganizationID,
                    BranchName = model.BranchName,
                    Email = model.Email,
                    Phone = model.Phone,
                    AddressLine1 = model.AddressLine1,
                    AddressLine2 = model.AddressLine2,
                    City = model.City,
                    State = model.State,
                    Country = model.Country,
                    Pincode = model.Pincode,
                    IsMainBranch = model.IsMainBranch,
                    IsActive = model.IsActive
                };

                await _uow.Branches.AddAsync(branch);
                return Json(new { success = true, message = "Branch created successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // GET: /Branches/Edit/5 (Partial View for Modal)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var branch = await _uow.Branches.GetByIdAsync(id);
            if (branch == null)
            {
                return NotFound("Branch not found.");
            }

            // Tenant Isolation check
            var userOrgId = User.GetOrganizationId();
            if (userOrgId.HasValue && branch.OrganizationID != userOrgId.Value)
            {
                return Forbid();
            }

            var model = new BranchViewModel
            {
                BranchID = branch.BranchID,
                OrganizationID = branch.OrganizationID,
                BranchName = branch.BranchName,
                Email = branch.Email,
                Phone = branch.Phone,
                AddressLine1 = branch.AddressLine1,
                AddressLine2 = branch.AddressLine2,
                City = branch.City,
                State = branch.State,
                Country = branch.Country,
                Pincode = branch.Pincode,
                IsMainBranch = branch.IsMainBranch,
                IsActive = branch.IsActive
            };

            if (User.IsPlatformOwner())
            {
                var orgs = await _uow.Organizations.GetAllAsync();
                ViewBag.Organizations = new SelectList(orgs.Where(o => o.IsActive), "OrganizationID", "OrganizationName", branch.OrganizationID);
            }

            return PartialView("_Edit", model);
        }

        // POST: /Branches/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BranchViewModel model)
        {
            // Security verification: If user is organization admin, ensure they don't modify another tenant
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
                var branch = await _uow.Branches.GetByIdAsync(model.BranchID);
                if (branch == null)
                {
                    return Json(new { success = false, message = "Branch not found." });
                }

                // Verify tenant boundaries
                if (userOrgId.HasValue && branch.OrganizationID != userOrgId.Value)
                {
                    return Json(new { success = false, message = "Unauthorized access." });
                }

                branch.OrganizationID = model.OrganizationID;
                branch.BranchName = model.BranchName;
                branch.Email = model.Email;
                branch.Phone = model.Phone;
                branch.AddressLine1 = model.AddressLine1;
                branch.AddressLine2 = model.AddressLine2;
                branch.City = model.City;
                branch.State = model.State;
                branch.Country = model.Country;
                branch.Pincode = model.Pincode;
                branch.IsMainBranch = model.IsMainBranch;
                branch.IsActive = model.IsActive;

                await _uow.Branches.UpdateAsync(branch);
                return Json(new { success = true, message = "Branch updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // POST: /Branches/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var branch = await _uow.Branches.GetByIdAsync(id);
                if (branch == null)
                {
                    return Json(new { success = false, message = "Branch not found." });
                }

                // Tenant Isolation check
                var userOrgId = User.GetOrganizationId();
                if (userOrgId.HasValue && branch.OrganizationID != userOrgId.Value)
                {
                    return Json(new { success = false, message = "Unauthorized access." });
                }

                var result = await _uow.Branches.DeleteAsync(id);
                if (result)
                {
                    return Json(new { success = true, message = "Branch deleted successfully." });
                }
                return Json(new { success = false, message = "Branch could not be deleted." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
