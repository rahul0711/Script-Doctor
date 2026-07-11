using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pharma_Script.Helpers;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.ViewModels.Receptionist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers
{
    [Authorize(Roles = "Platform Owner,Organization Admin")]
    public class ReceptionistsController : Controller
    {
        private readonly IUnitOfWork _uow;

        public ReceptionistsController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // GET: /Receptionists
        public async Task<IActionResult> Index(int? branchId, string searchTerm, int page = 1, int pageSize = 10)
        {
            var isPlatformOwner = User.IsPlatformOwner();
            var orgId = User.GetOrganizationId();

            IEnumerable<Branch> branches;
            if (isPlatformOwner)
            {
                branches = await _uow.Branches.GetAllAsync();
            }
            else if (orgId.HasValue)
            {
                branches = await _uow.Branches.GetByOrganizationIdAsync(orgId.Value);
            }
            else
            {
                branches = new List<Branch>();
            }

            ViewBag.Branches = new SelectList(branches.Where(b => b.IsActive == true), "BranchID", "BranchName", branchId);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedBranch = branchId;

            var receptionists = await _uow.Receptionists.SearchAndPaginateAsync(isPlatformOwner ? null : orgId, branchId, searchTerm, page, pageSize);
            var totalItems = await _uow.Receptionists.GetSearchCountAsync(isPlatformOwner ? null : orgId, branchId, searchTerm);

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(receptionists);
        }

        // GET: /Receptionists/Details/5 (Partial View Modal)
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userOrgId = User.GetOrganizationId();
            var recep = await _uow.Receptionists.GetByIdAsync(id);
            if (recep == null || (!User.IsPlatformOwner() && userOrgId.HasValue && recep.OrganizationID != userOrgId.Value))
            {
                return NotFound("Receptionist not found.");
            }

            // Bind names manually
            var user = await _uow.Users.GetByIdAsync(recep.UserID);
            if (user != null)
            {
                recep.FirstName = user.FirstName;
                recep.LastName = user.LastName;
                recep.Email = user.Email;
                recep.Phone = user.Phone;
            }

            var branch = recep.BranchID.HasValue ? await _uow.Branches.GetByIdAsync(recep.BranchID.Value) : null;
            recep.BranchName = branch?.BranchName;

            var org = await _uow.Organizations.GetByIdAsync(recep.OrganizationID);
            recep.OrganizationName = org?.OrganizationName;

            return PartialView("_Details", recep);
        }

        // GET: /Receptionists/Create (Partial View Modal)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new ReceptionistViewModel();
            var isPlatformOwner = User.IsPlatformOwner();
            var userOrgId = User.GetOrganizationId();

            if (isPlatformOwner)
            {
                var orgs = await _uow.Organizations.GetAllAsync();
                ViewBag.Organizations = new SelectList(orgs.Where(o => o.IsActive), "OrganizationID", "OrganizationName");
                ViewBag.Branches = new SelectList(new List<Branch>(), "BranchID", "BranchName");
            }
            else if (userOrgId.HasValue)
            {
                model.OrganizationID = userOrgId.Value;
                var branches = await _uow.Branches.GetByOrganizationIdAsync(userOrgId.Value);
                ViewBag.Branches = new SelectList(branches.Where(b => b.IsActive == true), "BranchID", "BranchName");
            }

            return PartialView("_Create", model);
        }

        // POST: /Receptionists/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReceptionistViewModel model)
        {
            var isPlatformOwner = User.IsPlatformOwner();
            var userOrgId = User.GetOrganizationId();

            if (!isPlatformOwner && userOrgId.HasValue)
            {
                model.OrganizationID = userOrgId.Value;
            }

            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", "Password is required for new receptionists.");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Validation failed.", errors });
            }

            if (model.BranchID.HasValue)
            {
                var branch = await _uow.Branches.GetByIdAsync(model.BranchID.Value);
                if (branch == null || branch.OrganizationID != model.OrganizationID)
                {
                    return Json(new { success = false, message = "Invalid branch selection." });
                }
            }

            // Duplicate Email
            var existingUser = await _uow.Users.GetByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return Json(new { success = false, message = "Email address already in use." });
            }

            var roles = await _uow.Roles.GetAllAsync();
            var recepRole = roles.FirstOrDefault(r => r.RoleName.Equals("Receptionist", StringComparison.OrdinalIgnoreCase));
            if (recepRole == null)
            {
                return Json(new { success = false, message = "Receptionist role not configured." });
            }

            await _uow.BeginTransactionAsync();
            try
            {
                // Create user
                var user = new User
                {
                    OrganizationID = model.OrganizationID,
                    RoleID = recepRole.RoleID,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone,
                    PasswordHash = PasswordHasher.HashPassword(model.Password!),
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.Now
                };

                var userId = await _uow.Users.AddAsync(user);

                // Create receptionist
                var receptionist = new Receptionist
                {
                    UserID = userId,
                    OrganizationID = model.OrganizationID,
                    BranchID = model.BranchID,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.Now
                };

                await _uow.Receptionists.AddAsync(receptionist);

                await _uow.CommitAsync();
                return Json(new { success = true, message = "Receptionist account created successfully." });
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // GET: /Receptionists/Edit/5 (Partial View Modal)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userOrgId = User.GetOrganizationId();
            var recep = await _uow.Receptionists.GetByIdAsync(id);
            if (recep == null || (!User.IsPlatformOwner() && userOrgId.HasValue && recep.OrganizationID != userOrgId.Value))
            {
                return NotFound("Receptionist profile not found.");
            }

            var user = await _uow.Users.GetByIdAsync(recep.UserID);
            if (user == null)
            {
                return NotFound("User profile not found.");
            }

            var model = new ReceptionistViewModel
            {
                ReceptionistID = recep.ReceptionistID,
                UserID = recep.UserID,
                OrganizationID = recep.OrganizationID,
                BranchID = recep.BranchID,
                IsActive = recep.IsActive,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone
            };

            var isPlatformOwner = User.IsPlatformOwner();
            if (isPlatformOwner)
            {
                var orgs = await _uow.Organizations.GetAllAsync();
                ViewBag.Organizations = new SelectList(orgs.Where(o => o.IsActive), "OrganizationID", "OrganizationName", recep.OrganizationID);
                var branches = await _uow.Branches.GetByOrganizationIdAsync(recep.OrganizationID);
                ViewBag.Branches = new SelectList(branches.Where(b => b.IsActive == true), "BranchID", "BranchName", recep.BranchID);
            }
            else if (userOrgId.HasValue)
            {
                var branches = await _uow.Branches.GetByOrganizationIdAsync(userOrgId.Value);
                ViewBag.Branches = new SelectList(branches.Where(b => b.IsActive == true), "BranchID", "BranchName", recep.BranchID);
            }

            return PartialView("_Edit", model);
        }

        // POST: /Receptionists/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ReceptionistViewModel model)
        {
            var isPlatformOwner = User.IsPlatformOwner();
            var userOrgId = User.GetOrganizationId();

            if (!isPlatformOwner && userOrgId.HasValue)
            {
                model.OrganizationID = userOrgId.Value;
            }

            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Validation failed.", errors });
            }

            if (model.BranchID.HasValue)
            {
                var branch = await _uow.Branches.GetByIdAsync(model.BranchID.Value);
                if (branch == null || branch.OrganizationID != model.OrganizationID)
                {
                    return Json(new { success = false, message = "Invalid branch selected." });
                }
            }

            var recep = await _uow.Receptionists.GetByIdAsync(model.ReceptionistID);
            if (recep == null || (userOrgId.HasValue && recep.OrganizationID != userOrgId.Value))
            {
                return Json(new { success = false, message = "Receptionist not found." });
            }

            // Duplicate Email Check
            var existingUser = await _uow.Users.GetByEmailAsync(model.Email);
            if (existingUser != null && existingUser.UserID != model.UserID)
            {
                return Json(new { success = false, message = "Email already in use." });
            }

            await _uow.BeginTransactionAsync();
            try
            {
                // Update User
                var user = await _uow.Users.GetByIdAsync(model.UserID);
                if (user != null)
                {
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.Email = model.Email;
                    user.Phone = model.Phone;
                    user.IsActive = model.IsActive;
                    await _uow.Users.UpdateAsync(user);
                }

                // Update Receptionist
                recep.BranchID = model.BranchID;
                recep.IsActive = model.IsActive;
                await _uow.Receptionists.UpdateAsync(recep);

                await _uow.CommitAsync();
                return Json(new { success = true, message = "Receptionist updated successfully." });
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // POST: /Receptionists/ToggleStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            var userOrgId = User.GetOrganizationId();
            var recep = await _uow.Receptionists.GetByIdAsync(id);
            if (recep == null || (userOrgId.HasValue && recep.OrganizationID != userOrgId.Value))
            {
                return Json(new { success = false, message = "Receptionist not found." });
            }

            await _uow.BeginTransactionAsync();
            try
            {
                await _uow.Receptionists.UpdateStatusAsync(id, isActive);
                await _uow.Users.UpdateStatusAsync(recep.UserID, isActive);

                await _uow.CommitAsync();
                return Json(new { success = true, message = $"Receptionist status {(isActive ? "activated" : "deactivated")} successfully." });
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return Json(new { success = false, message = $"Failed to change status: {ex.Message}" });
            }
        }

        // POST: /Receptionists/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userOrgId = User.GetOrganizationId();
            var recep = await _uow.Receptionists.GetByIdAsync(id);
            if (recep == null || (userOrgId.HasValue && recep.OrganizationID != userOrgId.Value))
            {
                return Json(new { success = false, message = "Receptionist not found." });
            }

            await _uow.BeginTransactionAsync();
            try
            {
                await _uow.Receptionists.DeleteAsync(id);
                await _uow.Users.DeleteAsync(recep.UserID);

                await _uow.CommitAsync();
                return Json(new { success = true, message = "Receptionist profile and account deleted." });
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return Json(new { success = false, message = $"Deletion failed: {ex.Message}" });
            }
        }
    }
}
