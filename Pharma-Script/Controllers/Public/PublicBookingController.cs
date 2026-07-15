using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharma_Script.Helpers;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.Services.Interfaces;
using Pharma_Script.ViewModels.Appointment;
using Pharma_Script.ViewModels.Public;
using System;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers.Public
{
    // Thin public wrapper around the existing Phase 3 appointment booking engine
    // (IAppointmentService). No slot generation, fee, or availability logic lives
    // here - it all still runs through AppointmentService.BookAppointmentAsync,
    // exactly as it does for the internal /Appointments/Book flow.
    //
    // Gated to the Patient role at the controller level so ASP.NET Core's cookie
    // auth challenges anonymous visitors the moment they click "Book Appointment" -
    // BEFORE they fill in any booking details - and redirects them straight back
    // here (slug + doctorId preserved in ReturnUrl) once they log in or register.
    [Authorize(Roles = "Patient")]
    [Route("{slug:activeOrgSlug}/doctors/{doctorId:int}/book")]
    public class PublicBookingController : PublicControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public PublicBookingController(IUnitOfWork uow, IAppointmentService appointmentService) : base(uow)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Book(int doctorId)
        {
            var mismatch = await ValidateContextAsync(doctorId);
            if (mismatch != null) return mismatch;

            var doctor = await Uow.Doctors.GetDoctorDetailsByIdAsync(doctorId, OrganizationId);

            var model = new AppointmentBookingViewModel
            {
                DoctorID = doctorId,
                AppointmentDate = DateTime.Today
            };

            var vm = new PublicBookingViewModel
            {
                Tenant = Tenant,
                Doctor = doctor!,
                Booking = model
            };

            ViewData["Title"] = $"Book Dr. {doctor!.FirstName} {doctor.LastName}";
            return View(vm);
        }

        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(int doctorId, [Bind(Prefix = "Booking")] AppointmentBookingViewModel booking)
        {
            var mismatch = await ValidateContextAsync(doctorId);
            if (mismatch != null) return mismatch;

            var patient = await Uow.Patients.GetByUserIdAsync(User.GetUserId());
            if (patient == null || patient.OrganizationID != OrganizationId)
            {
                return Forbid();
            }

            booking.DoctorID = doctorId;
            booking.PatientID = patient.PatientID;

            if (!ModelState.IsValid)
            {
                var doctor = await Uow.Doctors.GetDoctorDetailsByIdAsync(doctorId, OrganizationId);
                var vm = new PublicBookingViewModel { Tenant = Tenant, Doctor = doctor!, Booking = booking };
                TempData["Error"] = "Please complete all required fields correctly.";
                return View(vm);
            }

            try
            {
                var doctorRecord = await Uow.Doctors.GetByIdAsync(doctorId);
                var appt = await _appointmentService.BookAppointmentAsync(booking, OrganizationId, doctorRecord?.BranchID);

                var slug = Tenant.Organization.OrganizationSlug;
                return RedirectToAction("Confirmation", new { slug, doctorId, appointmentId = appt.AppointmentID });
            }
            catch (Exception ex)
            {
                var doctor = await Uow.Doctors.GetDoctorDetailsByIdAsync(doctorId, OrganizationId);
                var vm = new PublicBookingViewModel { Tenant = Tenant, Doctor = doctor!, Booking = booking };
                TempData["Error"] = ex.Message;
                return View(vm);
            }
        }

        [HttpGet("confirmation/{appointmentId:int}")]
        public async Task<IActionResult> Confirmation(int doctorId, int appointmentId)
        {
            var appt = await Uow.Appointments.GetAppointmentDetailsByIdAsync(appointmentId, OrganizationId);
            if (appt == null || appt.DoctorID != doctorId)
            {
                return NotFound();
            }

            ViewData["Title"] = "Appointment Confirmed";
            return View(appt);
        }

        // Verifies the doctor belongs to the resolved tenant AND, if the visitor is
        // already logged in, that their patient account belongs to this same tenant -
        // preventing a patient from one organization from booking against another.
        private async Task<IActionResult?> ValidateContextAsync(int doctorId)
        {
            var doctor = await Uow.Doctors.GetDoctorDetailsByIdAsync(doctorId, OrganizationId);
            if (doctor == null || !doctor.IsActive)
            {
                return NotFound();
            }

            var patient = await Uow.Patients.GetByUserIdAsync(User.GetUserId());
            if (patient != null && patient.OrganizationID != OrganizationId)
            {
                TempData["Error"] = "Your account belongs to a different organization's patient portal and cannot book here.";
                return RedirectToAction("Index", "PublicHome", new { slug = Tenant.Organization.OrganizationSlug });
            }

            return null;
        }
    }
}
