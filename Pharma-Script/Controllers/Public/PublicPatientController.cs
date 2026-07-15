using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharma_Script.Helpers;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.ViewModels.Public;
using System;
using System.Collections.Generic;
using System.Linq;
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
