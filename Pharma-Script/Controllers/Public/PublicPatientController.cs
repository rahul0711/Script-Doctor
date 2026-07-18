using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharma_Script.Helpers;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.ViewModels.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers.Public
{
    // Patient portal pages — embedded inside the CMS public layout.
    // Every action double-validates:
    //   1. Patient.OrganizationID == OrganizationId (from slug, never from client)
    //   2. PatientID comes ONLY from the authenticated session claim
    //
    // This prevents IDOR and cross-tenant access entirely.
    [Authorize(Roles = "Patient")]
    [Route("{slug:activeOrgSlug}/my")]
    public class PublicPatientController : PublicControllerBase
    {
        public PublicPatientController(IUnitOfWork uow) : base(uow)
        {
        }

        // GET /{slug}/my/appointments
        [HttpGet("appointments")]
        public async Task<IActionResult> MyAppointments(string? status = null, int page = 1)
        {
            var patient = await GetValidatedPatientAsync();
            if (patient == null) return Forbid();

            const int pageSize = 10;
            if (page < 1) page = 1;

            var appointments = await Uow.Appointments.SearchAndPaginateAsync(
                orgId: OrganizationId,
                branchId: null,
                doctorId: null,
                patientId: patient.PatientID,
                status: status,
                type: null,
                startDate: null,
                endDate: null,
                isPriority: null,
                searchTerm: null,
                page: page,
                pageSize: pageSize);

            var total = await Uow.Appointments.GetSearchCountAsync(
                orgId: OrganizationId,
                branchId: null,
                doctorId: null,
                patientId: patient.PatientID,
                status: status,
                type: null,
                startDate: null,
                endDate: null,
                isPriority: null,
                searchTerm: null);

            var vm = new PublicMyAppointmentsViewModel
            {
                Tenant = Tenant,
                Patient = patient,
                Appointments = appointments,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                TotalItems = total,
                PageSize = pageSize,
                StatusFilter = status
            };

            ViewData["Title"] = "My Appointments";
            return View(vm);
        }

        // GET /{slug}/my/history
        [HttpGet("history")]
        public async Task<IActionResult> MyHistory()
        {
            var patient = await GetValidatedPatientAsync();
            if (patient == null) return Forbid();

            // Fetch all completed appointments for this patient in this org (most recent first)
            // We pull up to 100 entries for the timeline; performance is fine for personal history.
            var appointments = (await Uow.Appointments.SearchAndPaginateAsync(
                orgId: OrganizationId,
                branchId: null,
                doctorId: null,
                patientId: patient.PatientID,
                status: null,
                type: null,
                startDate: null,
                endDate: null,
                isPriority: null,
                searchTerm: null,
                page: 1,
                pageSize: 100)).ToList();

            // Build the timeline — one entry per appointment, enriched with note/prescription/follow-up
            var timeline = new List<PatientTimelineEntry>();
            foreach (var appt in appointments)
            {
                var entry = new PatientTimelineEntry { Appointment = appt };

                // Doctor note — only show if note exists; ClinicalNotes (private) is NOT exposed
                var note = await Uow.DoctorNotes.GetByAppointmentIdAsync(appt.AppointmentID, OrganizationId);
                if (note != null)
                {
                    // Blank out private field before adding to view model
                    note.ClinicalNotes = null;
                    entry.Note = note;
                }

                // Prescription
                var prescription = await Uow.Prescriptions.GetByAppointmentIdAsync(appt.AppointmentID, OrganizationId);
                if (prescription != null)
                {
                    var medicines = await Uow.PrescriptionMedicines.GetByPrescriptionIdAsync(prescription.PrescriptionID);
                    entry.Prescription = prescription;
                    entry.Medicines = medicines.ToList();
                }

                // Follow-up
                var followUp = await Uow.FollowUps.GetByAppointmentIdAsync(appt.AppointmentID, OrganizationId);
                entry.FollowUp = followUp;

                timeline.Add(entry);
            }

            var vitals = await Uow.PatientVitals.GetLatestByPatientIdAsync(patient.PatientID);

            var vm = new PublicPatientHistoryViewModel
            {
                Tenant = Tenant,
                Patient = patient,
                Timeline = timeline,
                LatestVitals = vitals
            };

            ViewData["Title"] = "My Medical History";
            return View(vm);
        }

        // GET /{slug}/my/profile
        [HttpGet("profile")]
        public async Task<IActionResult> MyProfile()
        {
            var patient = await GetValidatedPatientAsync();
            if (patient == null) return Forbid();

            var vitals = await Uow.PatientVitals.GetLatestByPatientIdAsync(patient.PatientID);

            var vm = new PublicMyProfileViewModel
            {
                Tenant = Tenant,
                Patient = patient,
                LatestVitals = vitals
            };

            ViewData["Title"] = "My Profile";
            return View(vm);
        }

        // GET /{slug}/my/profile/edit
        [HttpGet("profile/edit")]
        public async Task<IActionResult> EditProfile()
        {
            var patient = await GetValidatedPatientAsync();
            if (patient == null) return Forbid();

            var vm = new PublicEditProfileViewModel
            {
                Tenant = Tenant,
                PatientID = patient.PatientID,
                UserID = patient.UserID,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Email = patient.Email,
                Phone = patient.Phone,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                BloodGroup = patient.BloodGroup,
                Height = patient.Height,
                Weight = patient.Weight,
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactNumber = patient.EmergencyContactNumber,
                Address = patient.Address,
                City = patient.City,
                State = patient.State,
                Country = patient.Country,
                Pincode = patient.Pincode
            };

            ViewData["Title"] = "Edit Profile";
            return View(vm);
        }

        // POST /{slug}/my/profile/edit
        [HttpPost("profile/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(PublicEditProfileViewModel model)
        {
            var patient = await GetValidatedPatientAsync();
            if (patient == null) return Forbid();

            if (patient.PatientID != model.PatientID || patient.UserID != model.UserID)
            {
                return Forbid();
            }

            ModelState.Remove("CurrentPassword");
            ModelState.Remove("NewPassword");
            ModelState.Remove("ConfirmNewPassword");

            var wantsPasswordChange = !string.IsNullOrWhiteSpace(model.NewPassword);

            if (!ModelState.IsValid)
            {
                model.Tenant = Tenant;
                TempData["Error"] = "Please correct the highlighted errors and try again.";
                ViewData["Title"] = "Edit Profile";
                return View(model);
            }

            var user = await Uow.Users.GetByIdAsync(patient.UserID);
            if (user == null)
            {
                TempData["Error"] = "User account not found.";
                return RedirectToAction("EditProfile");
            }

            var existingUser = await Uow.Users.GetByEmailAsync(model.Email);
            if (existingUser != null && existingUser.UserID != patient.UserID)
            {
                model.Tenant = Tenant;
                TempData["Error"] = "Email address is already in use by another account.";
                ViewData["Title"] = "Edit Profile";
                return View(model);
            }

            if (wantsPasswordChange)
            {
                if (string.IsNullOrWhiteSpace(model.CurrentPassword) || !PasswordHasher.VerifyPassword(model.CurrentPassword, user.PasswordHash))
                {
                    model.Tenant = Tenant;
                    TempData["Error"] = "Current password is incorrect. Password was not changed.";
                    ViewData["Title"] = "Edit Profile";
                    return View(model);
                }
            }

            await Uow.BeginTransactionAsync();
            try
            {
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.Phone = model.Phone;
                if (wantsPasswordChange)
                {
                    user.PasswordHash = PasswordHasher.HashPassword(model.NewPassword!);
                }
                await Uow.Users.UpdateAsync(user);

                patient.DateOfBirth = model.DateOfBirth;
                patient.Gender = model.Gender;
                patient.BloodGroup = model.BloodGroup;
                patient.Height = model.Height;
                patient.Weight = model.Weight;
                patient.EmergencyContactName = model.EmergencyContactName;
                patient.EmergencyContactNumber = model.EmergencyContactNumber;
                patient.Address = model.Address;
                patient.City = model.City;
                patient.State = model.State;
                patient.Country = model.Country;
                patient.Pincode = model.Pincode;

                await Uow.Patients.UpdateAsync(patient);

                await Uow.CommitAsync();

                // Refresh the auth cookie so the header name/email reflect the change immediately.
                await ReissueAuthCookieAsync(user);

                TempData["Success"] = wantsPasswordChange
                    ? "Profile and password updated successfully."
                    : "Profile updated successfully.";
                return RedirectToAction("MyProfile");
            }
            catch (Exception ex)
            {
                await Uow.RollbackAsync();
                model.Tenant = Tenant;
                TempData["Error"] = $"An error occurred: {ex.Message}";
                ViewData["Title"] = "Edit Profile";
                return View(model);
            }
        }

        private async Task ReissueAuthCookieAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, "Patient"),
                new Claim("FullName", $"{user.FirstName} {user.LastName}".Trim())
            };
            if (user.OrganizationID.HasValue)
            {
                claims.Add(new Claim("OrganizationID", user.OrganizationID.Value.ToString()));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var currentAuth = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = currentAuth.Properties ?? new AuthenticationProperties { ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2) };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), authProperties);
        }

        // ------------------------------------------------------------------
        // Security helper — resolves patient from session and verifies they
        // belong to the current CMS organization. Returns null if not valid.
        // PatientID is NEVER taken from the route or query string.
        // ------------------------------------------------------------------
        private async Task<Patient?> GetValidatedPatientAsync()
        {
            var userId = User.GetUserId();
            if (userId <= 0) return null;

            var patient = await Uow.Patients.GetByUserIdAsync(userId);
            if (patient == null) return null;

            // Tenant security — must belong to THIS organization
            if (patient.OrganizationID != OrganizationId) return null;

            return patient;
        }
    }
}
