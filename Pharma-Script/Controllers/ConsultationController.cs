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
        private readonly IConsultationSessionService _sessionService;
        private readonly IEmailService _emailService;

        public ConsultationController(IUnitOfWork uow, IAppointmentService appointmentService, IConsultationSessionService sessionService, IEmailService emailService)
        {
            _uow = uow;
            _appointmentService = appointmentService;
            _sessionService = sessionService;
            _emailService = emailService;
        }

        // GET: /Consultation
        public async Task<IActionResult> Index()
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

            var approvedAppts = await _uow.Appointments.SearchAndPaginateAsync(
                orgId: orgId.Value, branchId: null, doctorId: doctor.DoctorID, patientId: null,
                status: "Approved", type: null, startDate: null, endDate: null, isPriority: null,
                searchTerm: null, page: 1, pageSize: 100);

            var completedAppts = await _uow.Appointments.SearchAndPaginateAsync(
                orgId: orgId.Value, branchId: null, doctorId: doctor.DoctorID, patientId: null,
                status: "Completed", type: null, startDate: null, endDate: null, isPriority: null,
                searchTerm: null, page: 1, pageSize: 100);

            var list = approvedAppts.Concat(completedAppts)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.StartTime);

            return View(list);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveVideoLink(int appointmentId, string meetingProvider, string meetingUrl)
        {
            var orgId = User.GetOrganizationId();
            if (!orgId.HasValue) return BadRequest();

            try
            {
                await _sessionService.UpdateVideoLinkAsync(appointmentId, meetingProvider, meetingUrl, orgId.Value);
                TempData["Success"] = "Video link saved successfully.";

                var appt = await _uow.Appointments.GetAppointmentDetailsByIdAsync(appointmentId, orgId.Value);
                if (appt != null
                    && appt.AppointmentType.Equals("Video", StringComparison.OrdinalIgnoreCase)
                    && appt.PaymentStatus != null && appt.PaymentStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase))
                {
                    var patient = await _uow.Patients.GetPatientDetailsByIdAsync(appt.PatientID, orgId.Value);
                    if (patient != null && !string.IsNullOrWhiteSpace(patient.Email))
                    {
                        int? age = appt.PatientDOB.HasValue ? DateTime.Today.Year - appt.PatientDOB.Value.Year : null;

                        // Best-effort: an email failure should not undo the already-saved link.
                        try
                        {
                            await _emailService.SendVideoConsultationLinkEmailAsync(
                                patient.Email, appt.PatientName ?? "Patient", age, appt.DoctorName ?? "Doctor",
                                meetingUrl, meetingProvider, appt.AppointmentDate, appt.StartTime);
                            TempData["Success"] = $"Video link saved and emailed to {patient.Email}.";
                        }
                        catch (Exception emailEx)
                        {
                            Console.WriteLine($"Failed to email video link for AppointmentID {appointmentId}: {emailEx}");
                            TempData["Error"] = $"Link was saved, but the email to the patient failed: {emailEx.Message}";
                        }
                    }
                    else
                    {
                        TempData["Error"] = "Link was saved, but the patient has no email address on file, so no email was sent.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error saving link: {ex.Message}";
            }

            return RedirectToAction("Details", "Appointments", new { id = appointmentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartSession(int appointmentId)
        {
            var orgId = User.GetOrganizationId();
            if (!orgId.HasValue) return BadRequest();

            try
            {
                await _sessionService.UpdateSessionStatusAsync(appointmentId, "Started", orgId.Value);
                return RedirectToAction("Workspace", new { id = appointmentId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error starting session: {ex.Message}";
                return RedirectToAction("Details", "Appointments", new { id = appointmentId });
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
