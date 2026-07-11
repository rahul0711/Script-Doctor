using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharma_Script.Helpers;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers
{
    [Authorize]
    public class RolesController : Controller
    {
        private readonly IUnitOfWork _uow;

        public RolesController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // GET: /Roles
        public async Task<IActionResult> Index()
        {
            var roles = await _uow.Roles.GetAllAsync();
            return View(roles);
        }

        // GET: /Roles/Create (Partial View for Platform Owner)
        [HttpGet]
        [Authorize(Roles = "Platform Owner")]
        public IActionResult Create()
        {
            return PartialView("_Create", new Role());
        }

        // POST: /Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Platform Owner")]
        public async Task<IActionResult> Create(Role model)
        {
            if (string.IsNullOrWhiteSpace(model.RoleName))
            {
                return Json(new { success = false, message = "Role Name is required." });
            }

            try
            {
                var existing = await _uow.Roles.GetByNameAsync(model.RoleName);
                if (existing != null)
                {
                    return Json(new { success = false, message = "Role Name already exists." });
                }

                await _uow.Roles.AddAsync(model);
                return Json(new { success = true, message = "Role created successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // GET: /Roles/Edit/5 (Partial View for Platform Owner)
        [HttpGet]
        [Authorize(Roles = "Platform Owner")]
        public async Task<IActionResult> Edit(int id)
        {
            var role = await _uow.Roles.GetByIdAsync(id);
            if (role == null)
            {
                return NotFound("Role not found.");
            }
            return PartialView("_Edit", role);
        }

        // POST: /Roles/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Platform Owner")]
        public async Task<IActionResult> Edit(Role model)
        {
            if (string.IsNullOrWhiteSpace(model.RoleName))
            {
                return Json(new { success = false, message = "Role Name is required." });
            }

            try
            {
                var role = await _uow.Roles.GetByIdAsync(model.RoleID);
                if (role == null)
                {
                    return Json(new { success = false, message = "Role not found." });
                }

                role.RoleName = model.RoleName;
                role.Description = model.Description;

                await _uow.Roles.UpdateAsync(role);
                return Json(new { success = true, message = "Role updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
