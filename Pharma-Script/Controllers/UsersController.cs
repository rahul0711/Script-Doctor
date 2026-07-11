using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pharma_Script.Helpers;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers
{
    [Authorize(Roles = "Platform Owner,Organization Admin")]
    public class UsersController : Controller
    {
        private readonly IUnitOfWork _uow;

        public UsersController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // GET: /Users
        public async Task<IActionResult> Index()
        {
            var isPlatformOwner = User.IsPlatformOwner();
            var orgId = User.GetOrganizationId();

            IEnumerable<User> users;

            if (isPlatformOwner)
            {
                users = await _uow.Users.GetAllAsync();
            }
            else if (orgId.HasValue)
            {
                users = await _uow.Users.GetByOrganizationIdAsync(orgId.Value);
            }
            else
            {
                users = new List<User>();
            }

            return View(users);
        }

        // GET: /Users/Details/5 (Partial View)
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _uow.Users.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Tenant isolation check
            var userOrgId = User.GetOrganizationId();
            if (userOrgId.HasValue && user.OrganizationID != userOrgId.Value)
            {
                return Forbid();
            }

            return PartialView("_Details", user);
        }

        // GET: /Users/Create (Partial View)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new UserViewModel();
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

            // Load Roles (For Org Admin, exclude Platform Owner role)
            var roles = await _uow.Roles.GetAllAsync();
            if (!isPlatformOwner)
            {
                roles = roles.Where(r => !r.RoleName.Equals("Platform Owner", StringComparison.OrdinalIgnoreCase));
            }

            ViewBag.Roles = new SelectList(roles, "RoleID", "RoleName");

            return PartialView("_Create", model);
        }

        // POST: /Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            var isPlatformOwner = User.IsPlatformOwner();
            var userOrgId = User.GetOrganizationId();

            if (userOrgId.HasValue)
            {
                model.OrganizationID = userOrgId.Value;
            }

            // Password requirement on creation
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", "Password is required for new users.");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Validation failed.", errors });
            }

            try
            {
                // Check duplicate email
                var existingUser = await _uow.Users.GetByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return Json(new { success = false, message = "Email address is already in use by another user." });
                }

                // Verify Role allocation rules
                var role = await _uow.Roles.GetByIdAsync(model.RoleID);
                if (role == null || (!isPlatformOwner && role.RoleName.Equals("Platform Owner", StringComparison.OrdinalIgnoreCase)))
                {
                    return Json(new { success = false, message = "Invalid role assigned." });
                }

                var user = new User
                {
                    OrganizationID = model.OrganizationID,
                    RoleID = model.RoleID,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone,
                    PasswordHash = PasswordHasher.HashPassword(model.Password!),
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.Now
                };

                await _uow.Users.AddAsync(user);
                return Json(new { success = true, message = "User created successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // GET: /Users/Edit/5 (Partial View)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _uow.Users.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Tenant isolation check
            var userOrgId = User.GetOrganizationId();
            if (userOrgId.HasValue && user.OrganizationID != userOrgId.Value)
            {
                return Forbid();
            }

            var model = new UserViewModel
            {
                UserID = user.UserID,
                OrganizationID = user.OrganizationID,
                RoleID = user.RoleID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                IsActive = user.IsActive
            };

            var isPlatformOwner = User.IsPlatformOwner();
            if (isPlatformOwner)
            {
                var orgs = await _uow.Organizations.GetAllAsync();
                ViewBag.Organizations = new SelectList(orgs.Where(o => o.IsActive), "OrganizationID", "OrganizationName", user.OrganizationID);
            }

            var roles = await _uow.Roles.GetAllAsync();
            if (!isPlatformOwner)
            {
                roles = roles.Where(r => !r.RoleName.Equals("Platform Owner", StringComparison.OrdinalIgnoreCase));
            }
            ViewBag.Roles = new SelectList(roles, "RoleID", "RoleName", user.RoleID);

            return PartialView("_Edit", model);
        }

        // POST: /Users/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserViewModel model)
        {
            var isPlatformOwner = User.IsPlatformOwner();
            var userOrgId = User.GetOrganizationId();

            if (userOrgId.HasValue)
            {
                model.OrganizationID = userOrgId.Value;
            }

            // For editing, remove password validation errors since password is not modified here
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Validation failed.", errors });
            }

            try
            {
                var user = await _uow.Users.GetByIdAsync(model.UserID);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                // Tenant Isolation verify
                if (userOrgId.HasValue && user.OrganizationID != userOrgId.Value)
                {
                    return Json(new { success = false, message = "Unauthorized access." });
                }

                // Check duplicate email
                var existingUser = await _uow.Users.GetByEmailAsync(model.Email);
                if (existingUser != null && existingUser.UserID != user.UserID)
                {
                    return Json(new { success = false, message = "Email address is already in use by another user." });
                }

                // Verify Role allocation rules
                var role = await _uow.Roles.GetByIdAsync(model.RoleID);
                if (role == null || (!isPlatformOwner && role.RoleName.Equals("Platform Owner", StringComparison.OrdinalIgnoreCase)))
                {
                    return Json(new { success = false, message = "Invalid role assigned." });
                }

                user.OrganizationID = model.OrganizationID;
                user.RoleID = model.RoleID;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.Phone = model.Phone;
                user.IsActive = model.IsActive;

                await _uow.Users.UpdateAsync(user);
                return Json(new { success = true, message = "User updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // POST: /Users/ToggleStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            try
            {
                var user = await _uow.Users.GetByIdAsync(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                // Tenant isolation check
                var userOrgId = User.GetOrganizationId();
                if (userOrgId.HasValue && user.OrganizationID != userOrgId.Value)
                {
                    return Json(new { success = false, message = "Unauthorized access." });
                }

                // Prevent self-deactivation
                var loggedInUserId = User.GetUserId();
                if (loggedInUserId == id && !isActive)
                {
                    return Json(new { success = false, message = "You cannot deactivate your own account." });
                }

                var result = await _uow.Users.UpdateStatusAsync(id, isActive);
                if (result)
                {
                    return Json(new { success = true, message = $"User {(isActive ? "activated" : "deactivated")} successfully." });
                }
                return Json(new { success = false, message = "Status could not be changed." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // POST: /Users/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _uow.Users.GetByIdAsync(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                // Tenant isolation check
                var userOrgId = User.GetOrganizationId();
                if (userOrgId.HasValue && user.OrganizationID != userOrgId.Value)
                {
                    return Json(new { success = false, message = "Unauthorized access." });
                }

                // Prevent self-deletion
                var loggedInUserId = User.GetUserId();
                if (loggedInUserId == id)
                {
                    return Json(new { success = false, message = "You cannot delete your own account." });
                }

                var result = await _uow.Users.DeleteAsync(id);
                if (result)
                {
                    return Json(new { success = true, message = "User deleted successfully." });
                }
                return Json(new { success = false, message = "User could not be deleted." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // GET: /Users/ChangePassword/5 (Partial View Modal)
        [HttpGet]
        public IActionResult ChangePassword(int id)
        {
            var model = new ChangePasswordViewModel { UserID = id };
            return PartialView("_ChangePassword", model);
        }

        // POST: /Users/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            ModelState.Remove("CurrentPassword");
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Validation failed.", errors });
            }

            try
            {
                var user = await _uow.Users.GetByIdAsync(model.UserID);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                // Tenant isolation check
                var userOrgId = User.GetOrganizationId();
                if (userOrgId.HasValue && user.OrganizationID != userOrgId.Value)
                {
                    return Json(new { success = false, message = "Unauthorized access." });
                }

                // Verify current password (if changing self password. Wait, if admin is resetting someone else's password, they might not need the current password, but let's check: if UserID matches logged in user, require current password verification).
                var loggedInUserId = User.GetUserId();
                if (loggedInUserId == model.UserID)
                {
                    if (!PasswordHasher.VerifyPassword(model.CurrentPassword, user.PasswordHash))
                    {
                        return Json(new { success = false, message = "Incorrect current password." });
                    }
                }

                var hashed = PasswordHasher.HashPassword(model.NewPassword);
                await _uow.Users.UpdatePasswordAsync(model.UserID, hashed);

                return Json(new { success = true, message = "Password updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
