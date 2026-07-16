using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharma_Script.Helpers;
using Pharma_Script.Models;
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
        private readonly IPaymentService _paymentService;

        public PublicBookingController(IUnitOfWork uow, IAppointmentService appointmentService, IPaymentService paymentService) : base(uow)
        {
            _appointmentService = appointmentService;
            _paymentService = paymentService;
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

            var gateway = await Uow.DoctorPaymentGateways.GetByDoctorIdAsync(doctorId);

            var vm = new PublicBookingViewModel
            {
                Tenant = Tenant,
                Doctor = doctor!,
                Booking = model,
                PaymentGatewayAvailable = gateway != null && gateway.IsActive
            };

            ViewData["Title"] = $"Book Dr. {doctor!.FirstName} {doctor.LastName}";
            return View(vm);
        }

        // AJAX: called right before Razorpay Checkout is opened. Re-runs the same validation
        // BookAppointmentAsync does (without writing anything) to resolve the authoritative
        // fee, then asks Razorpay to create an Order for that amount.
        [HttpPost("create-order")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(int doctorId, [Bind(Prefix = "Booking")] AppointmentBookingViewModel booking)
        {
            var mismatch = await ValidateContextAsync(doctorId);
            if (mismatch != null)
            {
                return Json(new { success = false, message = "Unable to validate booking context." });
            }

            var patient = await Uow.Patients.GetByUserIdAsync(User.GetUserId());
            if (patient == null || patient.OrganizationID != OrganizationId)
            {
                return Json(new { success = false, message = "Patient account not recognized." });
            }

            booking.DoctorID = doctorId;
            booking.PatientID = patient.PatientID;

            var gateway = await Uow.DoctorPaymentGateways.GetByDoctorIdAsync(doctorId);
            if (gateway == null || !gateway.IsActive)
            {
                return Json(new { success = false, message = "Online payment is not available for this doctor right now. Please contact the clinic to book your appointment." });
            }

            try
            {
                var fee = await _appointmentService.QuoteFeeAsync(booking, OrganizationId);
                var receipt = $"appt-{doctorId}-{patient.PatientID}-{DateTime.UtcNow.Ticks}";
                var order = await _paymentService.CreateOrderAsync(gateway.KeyID, gateway.KeySecret, fee, receipt);

                return Json(new
                {
                    success = true,
                    orderId = order.OrderId,
                    amount = order.AmountPaise,
                    currency = order.Currency,
                    keyId = order.KeyId,
                    name = $"{patient.FirstName} {patient.LastName}".Trim(),
                    email = patient.Email,
                    contact = patient.Phone
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
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

            if (!ModelState.IsValid
                || string.IsNullOrWhiteSpace(booking.RazorpayOrderId)
                || string.IsNullOrWhiteSpace(booking.RazorpayPaymentId)
                || string.IsNullOrWhiteSpace(booking.RazorpaySignature))
            {
                var doctor = await Uow.Doctors.GetDoctorDetailsByIdAsync(doctorId, OrganizationId);
                var vm = new PublicBookingViewModel { Tenant = Tenant, Doctor = doctor!, Booking = booking };
                TempData["Error"] = ModelState.IsValid
                    ? "Payment was not completed. Please try again."
                    : "Please complete all required fields correctly.";
                return View(vm);
            }

            // Idempotency: a resubmitted successful payment (double-click / back-button) should
            // land on the existing confirmation instead of re-verifying and re-booking.
            var existingPayment = await Uow.Payments.GetByTransactionReferenceAsync(booking.RazorpayPaymentId);
            if (existingPayment?.AppointmentID != null)
            {
                var existingSlug = Tenant.Organization.OrganizationSlug;
                return RedirectToAction("Confirmation", new { slug = existingSlug, doctorId, appointmentId = existingPayment.AppointmentID.Value });
            }

            // Fetched regardless of IsActive: verification of an already-captured payment must still
            // succeed even if an admin disables online payments for this doctor in the interim.
            var gateway = await Uow.DoctorPaymentGateways.GetByDoctorIdAsync(doctorId);
            if (gateway == null)
            {
                var doctorForError = await Uow.Doctors.GetDoctorDetailsByIdAsync(doctorId, OrganizationId);
                var vmForError = new PublicBookingViewModel { Tenant = Tenant, Doctor = doctorForError!, Booking = booking };
                TempData["Error"] = "Payment configuration for this doctor is missing. Please contact support - your payment may still have been captured.";
                return View(vmForError);
            }

            var verification = await _paymentService.VerifyAndFetchAsync(gateway.KeyID, gateway.KeySecret, booking.RazorpayOrderId, booking.RazorpayPaymentId, booking.RazorpaySignature);
            if (!verification.IsValid)
            {
                var doctor = await Uow.Doctors.GetDoctorDetailsByIdAsync(doctorId, OrganizationId);
                var vm = new PublicBookingViewModel { Tenant = Tenant, Doctor = doctor!, Booking = booking };
                TempData["Error"] = verification.FailureReason ?? "Payment verification failed. Please try again.";
                return View(vm);
            }

            var payment = new Payment
            {
                Amount = verification.AmountPaise / 100m,
                PaymentMethod = verification.PaymentMethod ?? "Razorpay",
                TransactionReference = booking.RazorpayPaymentId,
                PaymentStatus = "Paid",
                PaidAt = DateTime.Now,
                Currency = "INR",
                RazorpayOrderId = booking.RazorpayOrderId,
                RazorpaySignature = booking.RazorpaySignature
            };

            try
            {
                var doctorRecord = await Uow.Doctors.GetByIdAsync(doctorId);
                var appt = await _appointmentService.BookAppointmentAsync(booking, OrganizationId, doctorRecord?.BranchID, payment);

                var slug = Tenant.Organization.OrganizationSlug;
                return RedirectToAction("Confirmation", new { slug, doctorId, appointmentId = appt.AppointmentID });
            }
            catch (Exception ex)
            {
                // Razorpay already captured the money at this point (verification passed above),
                // but the booking itself failed - e.g. a slot conflict discovered at insert time.
                // Persist the payment on its own (AppointmentID left null) so it isn't silently
                // lost and can be reconciled/refunded manually.
                try
                {
                    payment.OrganizationID = OrganizationId;
                    await Uow.Payments.AddAsync(payment);
                }
                catch (Exception persistEx)
                {
                    Console.WriteLine($"Failed to persist orphaned Razorpay payment {booking.RazorpayPaymentId}: {persistEx.Message}");
                }

                var doctor = await Uow.Doctors.GetDoctorDetailsByIdAsync(doctorId, OrganizationId);
                var vm = new PublicBookingViewModel { Tenant = Tenant, Doctor = doctor!, Booking = booking };
                TempData["Error"] = $"Your payment was received, but we could not confirm the booking ({ex.Message}). Our team will contact you shortly.";
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

            ViewBag.Payment = await Uow.Payments.GetByAppointmentIdAsync(appointmentId, OrganizationId);

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
