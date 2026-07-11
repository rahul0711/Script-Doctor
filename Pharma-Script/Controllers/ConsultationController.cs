using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharma_Script.Helpers;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.Services.Interfaces;
using Pharma_Script.ViewModels.Consultation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class ConsultationController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IAppointmentService _appointmentService;

        public ConsultationController(IUnitOfWork uow, IAppointmentService appointmentService)
        {
            _uow = uow;
            _appointmentService = appointmentService;
        }

        // GET: /Consultation/Workspace/{id}
        public async Task<IActionResult> Workspace(int id)
        {
            var orgId = User.GetOrganizationId();
            if (!orgId.HasValue) return BadRequest("Tenant context not resolved.");

            var userId = User.GetUserId();
            var doctor = await _uow.Doctors.GetByUserIdAsync(userId);
            if (doctor == null)
            {
                TempData["Error"] = "Doctor record not found.";
                return RedirectToAction("Index", "Dashboard");
            }

            var appt = await _uow.Appointments.GetAppointmentDetailsByIdAsync(id, orgId.Value);
            if (appt == null || appt.DoctorID != doctor.DoctorID)
            {
                return NotFound("Appointment not found or unauthorized access.");
            }

            if (!appt.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase) && 
                !appt.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Consultation can only be conducted for Approved appointments.";
                return RedirectToAction("Details", "Appointments", new { id = appt.AppointmentID });
            }

            // LEFT PANEL Data: Patient Summary
            var patient = await _uow.Patients.GetPatientDetailsByIdAsync(appt.PatientID, orgId.Value);
            var latestVitals = await _uow.PatientVitals.GetLatestByPatientIdAsync(appt.PatientID);
            var medicalHistory = await _uow.PatientMedicalHistories.GetByPatientIdAsync(appt.PatientID);
            var medicalDocs = await _uow.MedicalDocuments.GetByPatientIdAsync(appt.PatientID, orgId.Value);

            // RIGHT PANEL Data: Medical History context
            var pastPrescriptions = await _uow.Prescriptions.GetHistoryByPatientIdAsync(appt.PatientID, orgId.Value);
            
            // Build Model representing current Consultation Workspace state
            var notes = await _uow.DoctorNotes.GetByAppointmentIdAsync(id, orgId.Value);
            var prescription = await _uow.Prescriptions.GetByAppointmentIdAsync(id, orgId.Value);
            var medicines = new List<MedicineViewModel>();

            if (prescription != null)
            {
                var meds = await _uow.PrescriptionMedicines.GetByPrescriptionIdAsync(prescription.PrescriptionID);
                medicines = meds.Select(m => new MedicineViewModel
                {
                    MedicineName = m.MedicineName,
                    Strength = m.Strength,
                    Dosage = m.Dosage,
                    Morning = m.Morning,
                    Afternoon = m.Afternoon,
                    Night = m.Night,
                    BeforeFood = m.BeforeFood,
                    AfterFood = m.AfterFood,
                    DurationDays = m.DurationDays,
                    Quantity = m.Quantity,
                    Remarks = m.Remarks
                }).ToList();
            }

            var model = new ConsultationWorkspaceViewModel
            {
                AppointmentID = id,
                ClinicalNotes = notes?.ClinicalNotes,
                Diagnosis = notes?.Diagnosis ?? string.Empty,
                Advice = notes?.Advice,
                CreatePrescription = prescription != null,
                GeneralInstructions = prescription?.GeneralInstructions,
                NextVisitDate = prescription?.NextVisitDate,
                Medicines = medicines
            };

            // Inject ViewBag properties
            ViewBag.Appointment = appt;
            ViewBag.Patient = patient;
            ViewBag.Vitals = latestVitals;
            ViewBag.MedicalHistory = medicalHistory;
            ViewBag.MedicalDocs = medicalDocs;
            ViewBag.PastPrescriptions = pastPrescriptions;

            return View(model);
        }

        // POST: /Consultation/SaveWorkspace
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveWorkspace(ConsultationWorkspaceViewModel model)
        {
            var orgId = User.GetOrganizationId();
            if (!orgId.HasValue) return BadRequest("Tenant context not resolved.");

            var userId = User.GetUserId();
            var doctor = await _uow.Doctors.GetByUserIdAsync(userId);
            if (doctor == null)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Diagnosis is required to submit the consultation.";
                return RedirectToAction("Workspace", new { id = model.AppointmentID });
            }

            try
            {
                await _appointmentService.SaveConsultationWorkspaceAsync(model, doctor.DoctorID, orgId.Value);
                TempData["Success"] = "Consultation recorded successfully!";
                return RedirectToAction("Details", "Appointments", new { id = model.AppointmentID });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to record consultation: {ex.Message}";
                return RedirectToAction("Workspace", new { id = model.AppointmentID });
            }
        }

        // GET: /Consultation/PrintPrescription/{id}
        [AllowAnonymous] // Allow viewing for print sharing if needed, or secure by claim later
        public async Task<IActionResult> PrintPrescription(int id)
        {
            var appt = await _uow.Appointments.GetAppointmentDetailsByIdAsync(id, null);
            if (appt == null)
            {
                return NotFound("Appointment not found.");
            }

            var prescription = await _uow.Prescriptions.GetByAppointmentIdAsync(id, null);
            if (prescription == null)
            {
                return NotFound("Prescription not found for this appointment.");
            }

            var medicines = await _uow.PrescriptionMedicines.GetByPrescriptionIdAsync(prescription.PrescriptionID);
            var doctor = await _uow.Doctors.GetDoctorDetailsByIdAsync(appt.DoctorID, null);
            var patient = await _uow.Patients.GetPatientDetailsByIdAsync(appt.PatientID, null);
            var notes = await _uow.DoctorNotes.GetByAppointmentIdAsync(id, null);

            ViewBag.Appointment = appt;
            ViewBag.Prescription = prescription;
            ViewBag.Medicines = medicines;
            ViewBag.Doctor = doctor;
            ViewBag.Patient = patient;
            ViewBag.Notes = notes;

            return View();
        }
    }
}
