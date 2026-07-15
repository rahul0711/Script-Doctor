using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharma_Script.Helpers;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.ViewModels.Doctor;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers
{
    [Authorize(Roles = "Platform Owner,Organization Admin,Doctor")]
    public class DoctorLeaveController : Controller
    {
        private readonly IUnitOfWork _uow;

        public DoctorLeaveController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // GET: /DoctorLeave?doctorId=5
        [HttpGet]
        public async Task<IActionResult> Index(int doctorId)
        {
            var userOrgId = User.GetOrganizationId();
            var userId    = User.GetUserId();

            var doc = await _uow.Doctors.GetDoctorDetailsByIdAsync(doctorId, User.IsPlatformOwner() ? null : userOrgId);
            if (doc == null)
                return NotFound("Doctor profile not found.");

            // Doctors may only manage their own leaves
            if (User.IsDoctor() && doc.UserID != userId)
                return Forbid();

            var upcomingLeaves = await _uow.DoctorLeaves.GetUpcomingLeavesByDoctorIdAsync(doctorId);
            var pastLeaves     = await _uow.DoctorLeaves.GetPastLeavesByDoctorIdAsync(doctorId);

            ViewBag.Doctor        = doc;
            ViewBag.UpcomingLeaves = upcomingLeaves;
            ViewBag.PastLeaves    = pastLeaves;

            return View();
        }

        // GET: /DoctorLeave/Create (Partial View Modal)
        [HttpGet]
        public async Task<IActionResult> Create(int doctorId)
        {
            var userOrgId = User.GetOrganizationId();
            var userId    = User.GetUserId();
            var doc = await _uow.Doctors.GetByIdAsync(doctorId);
            if (doc == null
                || (!User.IsPlatformOwner() && !User.IsDoctor() && userOrgId.HasValue && doc.OrganizationID != userOrgId.Value)
                || (User.IsDoctor() && doc.UserID != userId))
            {
                return Forbid();
            }

            var model = new DoctorLeaveViewModel
            {
                DoctorID = doctorId,
                LeaveStartDate = DateTime.Today.AddDays(1),
                LeaveEndDate = DateTime.Today.AddDays(1)
            };

            return PartialView("_Create", model);
        }

        // POST: /DoctorLeave/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DoctorLeaveViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Validation failed.", errors });
            }

            var userOrgId = User.GetOrganizationId();
            var userId    = User.GetUserId();
            var doc = await _uow.Doctors.GetByIdAsync(model.DoctorID);
            if (doc == null
                || (!User.IsPlatformOwner() && !User.IsDoctor() && userOrgId.HasValue && doc.OrganizationID != userOrgId.Value)
                || (User.IsDoctor() && doc.UserID != userId))
            {
                return Json(new { success = false, message = "Unauthorized access." });
            }

            if (model.LeaveStartDate.Date > model.LeaveEndDate.Date)
            {
                return Json(new { success = false, message = "Leave start date cannot be after leave end date." });
            }

            // Check overlapping leave ranges
            var isOverlapping = await _uow.DoctorLeaves.CheckLeaveOverlapAsync(model.DoctorID, model.LeaveStartDate, model.LeaveEndDate, null);
            if (isOverlapping)
            {
                return Json(new { success = false, message = "The selected leave dates overlap with an existing leave record." });
            }

            try
            {
                var leave = new DoctorLeave
                {
                    DoctorID = model.DoctorID,
                    LeaveStartDate = model.LeaveStartDate,
                    LeaveEndDate = model.LeaveEndDate,
                    Reason = model.Reason
                };

                await _uow.DoctorLeaves.AddAsync(leave);
                return Json(new { success = true, message = "Leave record added successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // GET: /DoctorLeave/Edit/5 (Partial View Modal)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var leave = await _uow.DoctorLeaves.GetByIdAsync(id);
            if (leave == null)
            {
                return NotFound("Leave record not found.");
            }

            var userOrgId = User.GetOrganizationId();
            var userId    = User.GetUserId();
            var doc = await _uow.Doctors.GetByIdAsync(leave.DoctorID);
            if (doc == null
                || (!User.IsPlatformOwner() && !User.IsDoctor() && userOrgId.HasValue && doc.OrganizationID != userOrgId.Value)
                || (User.IsDoctor() && doc.UserID != userId))
            {
                return Forbid();
            }

            var model = new DoctorLeaveViewModel
            {
                LeaveID = leave.LeaveID,
                DoctorID = leave.DoctorID,
                LeaveStartDate = leave.LeaveStartDate,
                LeaveEndDate = leave.LeaveEndDate,
                Reason = leave.Reason
            };

            return PartialView("_Edit", model);
        }

        // POST: /DoctorLeave/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DoctorLeaveViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Validation failed.", errors });
            }

            var userOrgId = User.GetOrganizationId();
            var userId    = User.GetUserId();
            var doc = await _uow.Doctors.GetByIdAsync(model.DoctorID);
            if (doc == null
                || (!User.IsPlatformOwner() && !User.IsDoctor() && userOrgId.HasValue && doc.OrganizationID != userOrgId.Value)
                || (User.IsDoctor() && doc.UserID != userId))
            {
                return Json(new { success = false, message = "Unauthorized access." });
            }

            var leave = await _uow.DoctorLeaves.GetByIdAsync(model.LeaveID);
            if (leave == null)
            {
                return Json(new { success = false, message = "Leave record not found." });
            }

            if (model.LeaveStartDate.Date > model.LeaveEndDate.Date)
            {
                return Json(new { success = false, message = "Leave start date cannot be after leave end date." });
            }

            // Check overlapping leave ranges
            var isOverlapping = await _uow.DoctorLeaves.CheckLeaveOverlapAsync(model.DoctorID, model.LeaveStartDate, model.LeaveEndDate, model.LeaveID);
            if (isOverlapping)
            {
                return Json(new { success = false, message = "The selected leave dates overlap with an existing leave record." });
            }

            try
            {
                leave.LeaveStartDate = model.LeaveStartDate;
                leave.LeaveEndDate = model.LeaveEndDate;
                leave.Reason = model.Reason;

                await _uow.DoctorLeaves.UpdateAsync(leave);
                return Json(new { success = true, message = "Leave record updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // POST: /DoctorLeave/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var leave = await _uow.DoctorLeaves.GetByIdAsync(id);
            if (leave == null)
            {
                return Json(new { success = false, message = "Leave record not found." });
            }

            var userOrgId = User.GetOrganizationId();
            var userId    = User.GetUserId();
            var doc = await _uow.Doctors.GetByIdAsync(leave.DoctorID);
            if (doc == null
                || (!User.IsPlatformOwner() && !User.IsDoctor() && userOrgId.HasValue && doc.OrganizationID != userOrgId.Value)
                || (User.IsDoctor() && doc.UserID != userId))
            {
                return Json(new { success = false, message = "Unauthorized access." });
            }

            try
            {
                await _uow.DoctorLeaves.DeleteAsync(id);
                return Json(new { success = true, message = "Leave record deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
