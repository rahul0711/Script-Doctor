using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pharma_Script.Helpers;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.Services.Interfaces;
using Pharma_Script.ViewModels.Appointment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IUnitOfWork uow, IAppointmentService appointmentService)
        {
            _uow = uow;
            _appointmentService = appointmentService;
        }

        // GET: /Appointments
        public async Task<IActionResult> Index(
            int? doctorId, int? patientId, string? status, string? type, 
            DateTime? startDate, DateTime? endDate, bool? isPriority, 
            string? searchTerm, int page = 1, int pageSize = 10)
        {
            var isPlatformOwner = User.IsPlatformOwner();
            var orgId = User.GetOrganizationId();
            var role = User.GetRoleName();
            var userId = User.GetUserId();

            int? finalOrgId = isPlatformOwner ? null : orgId;
            int? finalBranchId = null;
            int? finalDoctorId = doctorId;
            int? finalPatientId = patientId;

            // Restrict views according to Roles
            if (role.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
            {
                var doc = await _uow.Doctors.GetByUserIdAsync(userId);
                if (doc == null)
                {
                    TempData["Error"] = "Doctor record not found.";
                    return RedirectToAction("Index", "Dashboard");
                }
                finalDoctorId = doc.DoctorID;
            }
            else if (role.Equals("Patient", StringComparison.OrdinalIgnoreCase))
            {
                var pat = await _uow.Patients.GetByUserIdAsync(userId);
                if (pat == null)
                {
                    TempData["Error"] = "Patient record not found.";
                    return RedirectToAction("Index", "Dashboard");
                }
                finalPatientId = pat.PatientID;
            }
            else if (role.Equals("Receptionist", StringComparison.OrdinalIgnoreCase))
            {
                var recep = await _uow.Receptionists.GetByUserIdAsync(userId);
                if (recep != null)
                {
                    finalBranchId = recep.BranchID; // Restrict Receptionist to their branch
                }
            }

            var appointments = await _uow.Appointments.SearchAndPaginateAsync(
                finalOrgId, finalBranchId, finalDoctorId, finalPatientId, 
                status, type, startDate, endDate, isPriority, searchTerm, page, pageSize);

            var totalCount = await _uow.Appointments.GetSearchCountAsync(
                finalOrgId, finalBranchId, finalDoctorId, finalPatientId, 
                status, type, startDate, endDate, isPriority, searchTerm);

            // Filters Metadata for UI
            if (isPlatformOwner)
            {
                ViewBag.Branches = new SelectList(await _uow.Branches.GetAllAsync(), "BranchID", "BranchName");
                ViewBag.Doctors = new SelectList(await _uow.Doctors.SearchAndPaginateAsync(null, null, null, null, true, "", 1, 100), "DoctorID", "FirstName");
            }
            else if (orgId.HasValue)
            {
                ViewBag.Branches = new SelectList(await _uow.Branches.GetByOrganizationIdAsync(orgId.Value), "BranchID", "BranchName");
                ViewBag.Doctors = new SelectList(await _uow.Doctors.SearchAndPaginateAsync(orgId.Value, null, null, null, true, "", 1, 100), "DoctorID", "FirstName");
            }

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            // Query parameters backup for pagination links
            ViewBag.DoctorId = doctorId;
            ViewBag.PatientId = patientId;
            ViewBag.Status = status;
            ViewBag.Type = type;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.IsPriority = isPriority;
            ViewBag.SearchTerm = searchTerm;

            return View(appointments);
        }

        // GET: /Appointments/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var orgId = User.IsPlatformOwner() ? null : User.GetOrganizationId();
            var appt = await _uow.Appointments.GetAppointmentDetailsByIdAsync(id, orgId);

            if (appt == null)
            {
                return NotFound("Appointment not found or access denied.");
            }

            // Enforce Role Access validation
            var role = User.GetRoleName();
            var userId = User.GetUserId();

            if (role.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
            {
                var doc = await _uow.Doctors.GetByUserIdAsync(userId);
                if (doc == null || appt.DoctorID != doc.DoctorID) return Forbid();
            }
            else if (role.Equals("Patient", StringComparison.OrdinalIgnoreCase))
            {
                var pat = await _uow.Patients.GetByUserIdAsync(userId);
                if (pat == null || appt.PatientID != pat.PatientID) return Forbid();
            }
            else if (role.Equals("Receptionist", StringComparison.OrdinalIgnoreCase))
            {
                var recep = await _uow.Receptionists.GetByUserIdAsync(userId);
                if (recep != null && recep.BranchID.HasValue && appt.BranchID != recep.BranchID.Value)
                {
                    return Forbid();
                }
            }

            // Load payments, doctor notes, prescription and status history
            ViewBag.Patient = await _uow.Patients.GetPatientDetailsByIdAsync(appt.PatientID, orgId);
            ViewBag.Payment = await _uow.Payments.GetByAppointmentIdAsync(id, orgId);
            ViewBag.DoctorNote = await _uow.DoctorNotes.GetByAppointmentIdAsync(id, orgId);
            ViewBag.Prescription = await _uow.Prescriptions.GetByAppointmentIdAsync(id, orgId);
            ViewBag.History = await _uow.AppointmentStatusHistories.GetByAppointmentIdAsync(id);
            ViewBag.ConsultationSession = await _uow.ConsultationSessions.GetByAppointmentIdAsync(id, orgId ?? appt.OrganizationID);

            return View(appt);
        }

        // GET: /Appointments/Book
        [Authorize(Roles = "Patient,Organization Admin,Receptionist")]
        public async Task<IActionResult> Book()
        {
            var orgId = User.GetOrganizationId();
            if (!orgId.HasValue)
            {
                TempData["Error"] = "Unable to resolve tenant organization.";
                return RedirectToAction("Index", "Dashboard");
            }

            // Check if patient is booking for themselves or admin/receptionist booking for a patient
            var role = User.GetRoleName();
            var userId = User.GetUserId();
            int patientId = 0;

            if (role.Equals("Patient", StringComparison.OrdinalIgnoreCase))
            {
                var pat = await _uow.Patients.GetByUserIdAsync(userId);
                if (pat == null)
                {
                    TempData["Error"] = "Patient profile setup is incomplete.";
                    return RedirectToAction("Index", "Dashboard");
                }
                patientId = pat.PatientID;
            }

            // Retrieve doctors list for Step 1 selection
            var doctors = await _uow.Doctors.SearchAndPaginateAsync(orgId.Value, null, null, null, true, "", 1, 100);
            var branches = await _uow.Branches.GetByOrganizationIdAsync(orgId.Value);
            var departments = await _uow.Departments.GetByOrganizationIdAsync(orgId.Value);
            var specializations = await _uow.Specializations.GetAllAsync();
            
            // Patients list for Admin/Receptionist booking
            if (!role.Equals("Patient", StringComparison.OrdinalIgnoreCase))
            {
                var patients = await _uow.Patients.SearchAndPaginateAsync(orgId.Value, null, "", 1, 500);
                ViewBag.Patients = new SelectList(patients.Select(p => new { p.PatientID, Name = $"{p.FirstName} {p.LastName} ({p.Email})" }), "PatientID", "Name");
            }

            ViewBag.DoctorsList = doctors;
            ViewBag.Branches = new SelectList(branches, "BranchID", "BranchName");
            ViewBag.Departments = new SelectList(departments, "DepartmentID", "DepartmentName");
            ViewBag.Specializations = new SelectList(specializations, "SpecializationID", "SpecializationName");
            ViewBag.PatientID = patientId;

            return View(new AppointmentBookingViewModel { PatientID = patientId, AppointmentDate = DateTime.Today });
        }

        // GET: /Appointments/GetAvailableSlots
        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetAvailableSlots(int doctorId, string date)
        {
            try
            {
                if (!DateTime.TryParse(date, out var parsedDate))
                {
                    return Json(new { success = false, message = "Invalid date format." });
                }

                var slots = await _appointmentService.GetAvailableSlotsAsync(doctorId, parsedDate);
                return Json(new { success = true, slots });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /Appointments/Book
        [HttpPost]
        [Authorize(Roles = "Patient,Organization Admin,Receptionist")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(AppointmentBookingViewModel model)
        {
            var orgId = User.GetOrganizationId();
            if (!orgId.HasValue)
            {
                return BadRequest("Invalid tenant context.");
            }

            var role = User.GetRoleName();
            var userId = User.GetUserId();

            // Override PatientID for patients to prevent spoofing
            if (role.Equals("Patient", StringComparison.OrdinalIgnoreCase))
            {
                var pat = await _uow.Patients.GetByUserIdAsync(userId);
                if (pat == null)
                {
                    ModelState.AddModelError("", "Patient profile not found.");
                }
                else
                {
                    model.PatientID = pat.PatientID;
                }
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields correctly.";
                return RedirectToAction("Book");
            }

            try
            {
                // Retrieve branch ID from doctor profile to store in appointment
                var doctor = await _uow.Doctors.GetByIdAsync(model.DoctorID);
                int? branchId = doctor?.BranchID;

                var appt = await _appointmentService.BookAppointmentAsync(model, orgId.Value, branchId);
                TempData["Success"] = "Appointment booked successfully!";
                return RedirectToAction("Details", new { id = appt.AppointmentID });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Book");
            }
        }

        // POST: /Appointments/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(AppointmentStatusChangeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid status update details.");
            }

            var orgId = User.IsPlatformOwner() ? null : User.GetOrganizationId();
            var userId = User.GetUserId();

            try
            {
                var ok = await _appointmentService.UpdateAppointmentStatusAsync(model.AppointmentID, model.NewStatus, userId, model.Remarks, orgId);
                if (ok)
                {
                    TempData["Success"] = $"Appointment status updated to '{model.NewStatus}'.";
                }
                else
                {
                    TempData["Error"] = "Unable to update status or record not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Details", new { id = model.AppointmentID });
        }

        // POST: /Appointments/Reschedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(AppointmentRescheduleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in rescheduled date and time slot.";
                return RedirectToAction("Details", new { id = model.AppointmentID });
            }

            var orgId = User.IsPlatformOwner() ? null : User.GetOrganizationId();
            var userId = User.GetUserId();

            try
            {
                var ok = await _appointmentService.RescheduleAppointmentAsync(model, userId, orgId);
                if (ok)
                {
                    TempData["Success"] = "Appointment rescheduled successfully.";
                }
                else
                {
                    TempData["Error"] = "Unable to reschedule appointment.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Details", new { id = model.AppointmentID });
        }

        // POST: /Appointments/AddPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPayment(int appointmentId, string paymentMethod, string transactionReference)
        {
            var orgId = User.GetOrganizationId();
            if (!orgId.HasValue) return BadRequest("Tenant context not resolved.");

            var appt = await _uow.Appointments.GetByIdAsync(appointmentId);
            if (appt == null || appt.OrganizationID != orgId.Value)
            {
                return NotFound("Appointment not found.");
            }

            // Create payment
            var payment = new Payment
            {
                AppointmentID = appointmentId,
                OrganizationID = orgId.Value,
                Amount = appt.ConsultationFee, // Server resolves from actual stored fee in appointment
                PaymentMethod = paymentMethod,
                TransactionReference = transactionReference,
                PaymentStatus = "Paid",
                PaidAt = DateTime.Now
            };

            await _uow.BeginTransactionAsync();
            try
            {
                await _uow.Payments.AddAsync(payment);
                
                // Update appointment log
                var history = new AppointmentStatusHistory
                {
                    AppointmentID = appointmentId,
                    OldStatus = appt.Status,
                    NewStatus = appt.Status, // Status remains same, but payment is recorded
                    ChangedByUserID = User.GetUserId(),
                    Remarks = $"Payment of INR {appt.ConsultationFee} logged successfully via {paymentMethod}."
                };
                await _uow.AppointmentStatusHistories.AddAsync(history);
                await _uow.CommitAsync();

                TempData["Success"] = "Payment captured successfully!";
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                TempData["Error"] = $"Payment capturing failed: {ex.Message}";
            }

            return RedirectToAction("Details", new { id = appointmentId });
        }
    }
}
