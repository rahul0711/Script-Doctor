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
    [Authorize(Roles = "Platform Owner,Organization Admin")]
    public class DoctorAvailabilityController : Controller
    {
        private readonly IUnitOfWork _uow;

        public DoctorAvailabilityController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // GET: /DoctorAvailability/Schedule?doctorId=5
        [HttpGet]
        public async Task<IActionResult> Schedule(int doctorId)
        {
            var userOrgId = User.GetOrganizationId();
            var doc = await _uow.Doctors.GetDoctorDetailsByIdAsync(doctorId, User.IsPlatformOwner() ? null : userOrgId);
            if (doc == null)
            {
                return NotFound("Doctor profile not found.");
            }

            var availabilities = await _uow.DoctorAvailabilities.GetAvailabilityByDoctorIdAsync(doctorId);

            ViewBag.Doctor = doc;
            return View(availabilities);
        }

        // GET: /DoctorAvailability/Create (Partial View Modal)
        [HttpGet]
        public async Task<IActionResult> Create(int doctorId)
        {
            var userOrgId = User.GetOrganizationId();
            var doc = await _uow.Doctors.GetByIdAsync(doctorId);
            if (doc == null || (!User.IsPlatformOwner() && userOrgId.HasValue && doc.OrganizationID != userOrgId.Value))
            {
                return Forbid();
            }

            var model = new DoctorAvailabilityViewModel
            {
                DoctorID = doctorId,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(17, 0, 0),
                SlotDuration = 15
            };

            return PartialView("_Create", model);
        }

        // POST: /DoctorAvailability/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DoctorAvailabilityViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Validation failed.", errors });
            }

            // Verify tenant boundaries
            var userOrgId = User.GetOrganizationId();
            var doc = await _uow.Doctors.GetByIdAsync(model.DoctorID);
            if (doc == null || (!User.IsPlatformOwner() && userOrgId.HasValue && doc.OrganizationID != userOrgId.Value))
            {
                return Json(new { success = false, message = "Unauthorized doctor access." });
            }

            // Check basic time range validation
            if (model.StartTime >= model.EndTime)
            {
                return Json(new { success = false, message = "Start time must be before end time." });
            }

            // Break validations
            if (model.BreakStart.HasValue || model.BreakEnd.HasValue)
            {
                if (!model.BreakStart.HasValue || !model.BreakEnd.HasValue)
                {
                    return Json(new { success = false, message = "Both break start and break end times are required if setting a break." });
                }

                if (model.BreakStart.Value >= model.BreakEnd.Value)
                {
                    return Json(new { success = false, message = "Break start time must be before break end time." });
                }

                // Check that break fits inside availability window
                if (model.BreakStart.Value < model.StartTime || model.BreakEnd.Value > model.EndTime)
                {
                    return Json(new { success = false, message = "Break times must fall inside the availability time range." });
                }
            }

            // Check overlapping availability for the same doctor and day
            var isOverlapping = await _uow.DoctorAvailabilities.CheckOverlapAsync(model.DoctorID, model.DayOfWeek, model.StartTime, model.EndTime, null);
            if (isOverlapping)
            {
                return Json(new { success = false, message = $"An availability slot already overlaps with the selected hours on {model.DayOfWeek}." });
            }

            try
            {
                var availability = new DoctorAvailability
                {
                    DoctorID = model.DoctorID,
                    DayOfWeek = model.DayOfWeek,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    SlotDuration = model.SlotDuration,
                    BreakStart = model.BreakStart,
                    BreakEnd = model.BreakEnd,
                    IsAvailable = model.IsAvailable
                };

                await _uow.DoctorAvailabilities.AddAsync(availability);
                return Json(new { success = true, message = "Availability slot created successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // GET: /DoctorAvailability/Edit/5 (Partial View Modal)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var avail = await _uow.DoctorAvailabilities.GetByIdAsync(id);
            if (avail == null)
            {
                return NotFound("Availability slot not found.");
            }

            // Tenant verification
            var userOrgId = User.GetOrganizationId();
            var doc = await _uow.Doctors.GetByIdAsync(avail.DoctorID);
            if (doc == null || (!User.IsPlatformOwner() && userOrgId.HasValue && doc.OrganizationID != userOrgId.Value))
            {
                return Forbid();
            }

            var model = new DoctorAvailabilityViewModel
            {
                AvailabilityID = avail.AvailabilityID,
                DoctorID = avail.DoctorID,
                DayOfWeek = avail.DayOfWeek,
                StartTime = avail.StartTime,
                EndTime = avail.EndTime,
                SlotDuration = avail.SlotDuration,
                BreakStart = avail.BreakStart,
                BreakEnd = avail.BreakEnd,
                IsAvailable = avail.IsAvailable
            };

            return PartialView("_Edit", model);
        }

        // POST: /DoctorAvailability/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DoctorAvailabilityViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Validation failed.", errors });
            }

            // Verify tenant boundaries
            var userOrgId = User.GetOrganizationId();
            var doc = await _uow.Doctors.GetByIdAsync(model.DoctorID);
            if (doc == null || (!User.IsPlatformOwner() && userOrgId.HasValue && doc.OrganizationID != userOrgId.Value))
            {
                return Json(new { success = false, message = "Unauthorized access." });
            }

            var avail = await _uow.DoctorAvailabilities.GetByIdAsync(model.AvailabilityID);
            if (avail == null)
            {
                return Json(new { success = false, message = "Availability slot not found." });
            }

            if (model.StartTime >= model.EndTime)
            {
                return Json(new { success = false, message = "Start time must be before end time." });
            }

            if (model.BreakStart.HasValue || model.BreakEnd.HasValue)
            {
                if (!model.BreakStart.HasValue || !model.BreakEnd.HasValue)
                {
                    return Json(new { success = false, message = "Both break start and break end times are required if setting a break." });
                }

                if (model.BreakStart.Value >= model.BreakEnd.Value)
                {
                    return Json(new { success = false, message = "Break start time must be before break end time." });
                }

                if (model.BreakStart.Value < model.StartTime || model.BreakEnd.Value > model.EndTime)
                {
                    return Json(new { success = false, message = "Break times must fall inside the availability time range." });
                }
            }

            // Check overlap
            var isOverlapping = await _uow.DoctorAvailabilities.CheckOverlapAsync(model.DoctorID, model.DayOfWeek, model.StartTime, model.EndTime, model.AvailabilityID);
            if (isOverlapping)
            {
                return Json(new { success = false, message = $"An availability slot already overlaps with the selected hours on {model.DayOfWeek}." });
            }

            try
            {
                avail.DayOfWeek = model.DayOfWeek;
                avail.StartTime = model.StartTime;
                avail.EndTime = model.EndTime;
                avail.SlotDuration = model.SlotDuration;
                avail.BreakStart = model.BreakStart;
                avail.BreakEnd = model.BreakEnd;
                avail.IsAvailable = model.IsAvailable;

                await _uow.DoctorAvailabilities.UpdateAsync(avail);
                return Json(new { success = true, message = "Availability slot updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // POST: /DoctorAvailability/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var avail = await _uow.DoctorAvailabilities.GetByIdAsync(id);
            if (avail == null)
            {
                return Json(new { success = false, message = "Availability slot not found." });
            }

            // Tenant verification
            var userOrgId = User.GetOrganizationId();
            var doc = await _uow.Doctors.GetByIdAsync(avail.DoctorID);
            if (doc == null || (!User.IsPlatformOwner() && userOrgId.HasValue && doc.OrganizationID != userOrgId.Value))
            {
                return Json(new { success = false, message = "Unauthorized access." });
            }

            try
            {
                await _uow.DoctorAvailabilities.DeleteAsync(id);
                return Json(new { success = true, message = "Availability slot deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
