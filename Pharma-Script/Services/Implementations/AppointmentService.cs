using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.Services.Interfaces;
using Pharma_Script.ViewModels.Appointment;
using Pharma_Script.ViewModels.Consultation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pharma_Script.Services.Implementations
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notificationService;

        public AppointmentService(IUnitOfWork uow, INotificationService notificationService)
        {
            _uow = uow;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<string>> GetAvailableSlotsAsync(int doctorId, DateTime date)
        {
            var dayOfWeek = date.ToString("dddd"); // Monday, Tuesday, etc.
            
            // 1. Get Doctor Availabilities
            var availabilities = await _uow.DoctorAvailabilities.GetAvailabilityByDoctorIdAsync(doctorId);
            var availability = availabilities.FirstOrDefault(a => 
                a.DayOfWeek.Equals(dayOfWeek, StringComparison.OrdinalIgnoreCase) && a.IsAvailable);
            
            if (availability == null)
            {
                return Enumerable.Empty<string>();
            }

            // 2. Check Doctor Leaves
            var leaves = await _uow.DoctorLeaves.GetUpcomingLeavesByDoctorIdAsync(doctorId);
            var pastLeaves = await _uow.DoctorLeaves.GetPastLeavesByDoctorIdAsync(doctorId);
            
            bool onLeave = leaves.Any(l => date.Date >= l.LeaveStartDate.Date && date.Date <= l.LeaveEndDate.Date)
                        || pastLeaves.Any(l => date.Date >= l.LeaveStartDate.Date && date.Date <= l.LeaveEndDate.Date);

            if (onLeave)
            {
                return Enumerable.Empty<string>();
            }

            // 3. Generate slots
            var slots = new List<string>();
            var startTime = availability.StartTime;
            var endTime = availability.EndTime;
            var duration = availability.SlotDuration > 0 ? availability.SlotDuration : 15;

            var current = startTime;
            while (current + TimeSpan.FromMinutes(duration) <= endTime)
            {
                var slotStart = current;
                var slotEnd = current + TimeSpan.FromMinutes(duration);

                // Check break time
                bool isInBreak = false;
                if (availability.BreakStart.HasValue && availability.BreakEnd.HasValue)
                {
                    if (slotStart < availability.BreakEnd.Value && slotEnd > availability.BreakStart.Value)
                    {
                        isInBreak = true;
                    }
                }

                if (!isInBreak)
                {
                    // For today's appointments, filter out past slots
                    if (date.Date == DateTime.Today)
                    {
                        if (slotStart <= DateTime.Now.TimeOfDay)
                        {
                            current = slotEnd;
                            continue;
                        }
                    }
                    slots.Add(slotStart.ToString(@"hh\:mm"));
                }
                current = slotEnd;
            }

            // 4. Remove booked slots
            var appointments = await _uow.Appointments.GetBookedSlotsAsync(doctorId, date);
            var availableSlots = new List<string>();

            foreach (var slotStr in slots)
            {
                var slotTime = TimeSpan.Parse(slotStr);
                var slotEnd = slotTime + TimeSpan.FromMinutes(duration);

                bool isBooked = false;
                foreach (var appt in appointments)
                {
                    if (slotTime < appt.EndTime && slotEnd > appt.StartTime)
                    {
                        isBooked = true;
                        break;
                    }
                }

                if (!isBooked)
                {
                    availableSlots.Add(slotStr);
                }
            }

            return availableSlots;
        }

        private class ValidatedBooking
        {
            public Doctor Doctor { get; set; } = null!;
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public decimal ConsultationFee { get; set; }
        }

        private async Task<ValidatedBooking> ValidateAndResolveAsync(AppointmentBookingViewModel model, int orgId)
        {
            if (model.AppointmentDate.Date < DateTime.Today)
            {
                throw new InvalidOperationException("Cannot book appointments for past dates.");
            }

            // 1. Resolve Doctor Fees & Availability
            var doctor = await _uow.Doctors.GetByIdAsync(model.DoctorID);
            if (doctor == null || doctor.OrganizationID != orgId)
            {
                throw new InvalidOperationException("Doctor not found or does not belong to this organization.");
            }

            var dayOfWeek = model.AppointmentDate.ToString("dddd");
            var availabilities = await _uow.DoctorAvailabilities.GetAvailabilityByDoctorIdAsync(model.DoctorID);
            var availability = availabilities.FirstOrDefault(a =>
                a.DayOfWeek.Equals(dayOfWeek, StringComparison.OrdinalIgnoreCase) && a.IsAvailable);

            if (availability == null)
            {
                throw new InvalidOperationException("Doctor is not available on this day of the week.");
            }

            // 2. Check Doctor Leaves
            var leaves = await _uow.DoctorLeaves.GetUpcomingLeavesByDoctorIdAsync(model.DoctorID);
            var pastLeaves = await _uow.DoctorLeaves.GetPastLeavesByDoctorIdAsync(model.DoctorID);
            bool onLeave = leaves.Any(l => model.AppointmentDate.Date >= l.LeaveStartDate.Date && model.AppointmentDate.Date <= l.LeaveEndDate.Date)
                        || pastLeaves.Any(l => model.AppointmentDate.Date >= l.LeaveStartDate.Date && model.AppointmentDate.Date <= l.LeaveEndDate.Date);

            if (onLeave)
            {
                throw new InvalidOperationException("Doctor is on leave on the selected date.");
            }

            // 3. Parse selected time slot
            if (!TimeSpan.TryParse(model.SelectedSlot, out var startTime))
            {
                throw new InvalidOperationException("Invalid time slot selected.");
            }
            var duration = availability.SlotDuration > 0 ? availability.SlotDuration : 15;
            var endTime = startTime + TimeSpan.FromMinutes(duration);

            // 4. Validate break times
            if (availability.BreakStart.HasValue && availability.BreakEnd.HasValue)
            {
                if (startTime < availability.BreakEnd.Value && endTime > availability.BreakStart.Value)
                {
                    throw new InvalidOperationException("Selected slot falls during the doctor's break time.");
                }
            }

            // 5. Check for double booking conflict
            var isConflicting = await _uow.Appointments.CheckSlotConflictAsync(model.DoctorID, model.AppointmentDate, startTime, endTime, null);
            if (isConflicting)
            {
                throw new InvalidOperationException("This time slot was just booked by another patient. Please select another available time.");
            }

            // 6. Resolve final fee server-side
            decimal consultationFee = 0;
            if (model.PriorityConsultation)
            {
                if (!doctor.IsPriorityAvailable)
                {
                    throw new InvalidOperationException("Priority consultation is not currently available for this doctor.");
                }
                consultationFee = doctor.PriorityConsultationFee;
            }
            else
            {
                if (model.AppointmentType.Equals("Video", StringComparison.OrdinalIgnoreCase))
                    consultationFee = doctor.VideoConsultationFee;
                else if (model.AppointmentType.Equals("Voice", StringComparison.OrdinalIgnoreCase))
                    consultationFee = doctor.VoiceConsultationFee;
                else
                    throw new InvalidOperationException("Invalid consultation type selection.");
            }

            return new ValidatedBooking
            {
                Doctor = doctor,
                StartTime = startTime,
                EndTime = endTime,
                ConsultationFee = consultationFee
            };
        }

        public async Task<decimal> QuoteFeeAsync(AppointmentBookingViewModel model, int orgId)
        {
            var validated = await ValidateAndResolveAsync(model, orgId);
            return validated.ConsultationFee;
        }

        public async Task<Appointment> BookAppointmentAsync(AppointmentBookingViewModel model, int orgId, int? branchId, Payment? payment = null)
        {
            var validated = await ValidateAndResolveAsync(model, orgId);
            var doctor = validated.Doctor;

            // Insert Appointment & Status Log inside a transaction
            var appt = new Appointment
            {
                OrganizationID = orgId,
                BranchID = branchId,
                DoctorID = model.DoctorID,
                PatientID = model.PatientID,
                AppointmentType = model.AppointmentType,
                AppointmentDate = model.AppointmentDate,
                StartTime = validated.StartTime,
                EndTime = validated.EndTime,
                ConsultationFee = validated.ConsultationFee,
                PriorityConsultation = model.PriorityConsultation,
                Symptoms = model.Symptoms,
                AppointmentReason = model.AppointmentReason,
                Status = "Pending"
            };

            await _uow.BeginTransactionAsync();
            try
            {
                var newId = await _uow.Appointments.AddAsync(appt);
                appt.AppointmentID = newId;

                // Log status history
                var patient = await _uow.Patients.GetByIdAsync(model.PatientID);
                if (patient == null)
                {
                    throw new InvalidOperationException("Patient not found.");
                }
                var history = new AppointmentStatusHistory
                {
                    AppointmentID = newId,
                    OldStatus = null,
                    NewStatus = "Pending",
                    ChangedByUserID = patient.UserID,
                    Remarks = "Appointment requested via Patient Portal."
                };
                await _uow.AppointmentStatusHistories.AddAsync(history);

                if (payment != null)
                {
                    payment.AppointmentID = newId;
                    payment.OrganizationID = orgId;
                    await _uow.Payments.AddAsync(payment);
                }

                await _uow.CommitAsync();

                // Notification is a best-effort side effect - the appointment is already
                // committed above, so a notification failure must never surface as a
                // booking failure to the patient.
                try
                {
                    await _notificationService.SendNotificationAsync(
                        doctor.UserID, orgId, "Appointment",
                        "New Appointment Request",
                        $"You have a new appointment request for {appt.StartTime:hh\\:mm} on {appt.AppointmentDate:MMM dd, yyyy}.",
                        "AppointmentID", appt.AppointmentID);
                }
                catch (Exception notifyEx)
                {
                    Console.WriteLine($"Notification failed for AppointmentID {appt.AppointmentID}: {notifyEx.Message}");
                }

                return appt;
            }
            catch
            {
                await _uow.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateAppointmentStatusAsync(int appointmentId, string newStatus, int changedByUserId, string? remarks, int? orgId)
        {
            var appt = await _uow.Appointments.GetByIdAsync(appointmentId);
            if (appt == null) return false;
            if (orgId.HasValue && appt.OrganizationID != orgId.Value) return false;

            // Validate status transitions
            if (!IsValidTransition(appt.Status, newStatus))
            {
                throw new InvalidOperationException($"Invalid status transition from {appt.Status} to {newStatus}.");
            }

            await _uow.BeginTransactionAsync();
            try
            {
                var ok = await _uow.Appointments.UpdateStatusAsync(appointmentId, newStatus);
                if (!ok)
                {
                    await _uow.RollbackAsync();
                    return false;
                }

                var history = new AppointmentStatusHistory
                {
                    AppointmentID = appointmentId,
                    OldStatus = appt.Status,
                    NewStatus = newStatus,
                    ChangedByUserID = changedByUserId,
                    Remarks = remarks ?? $"Status updated to {newStatus}."
                };
                await _uow.AppointmentStatusHistories.AddAsync(history);
                await _uow.CommitAsync();

                // Notification is a best-effort side effect - never surface its failure
                // as a failure of the already-committed status update.
                try
                {
                    var patient = await _uow.Patients.GetByIdAsync(appt.PatientID);
                    if (patient != null)
                    {
                        await _notificationService.SendNotificationAsync(
                            patient.UserID, appt.OrganizationID, "Appointment",
                            $"Appointment {newStatus}",
                            $"Your appointment for {appt.AppointmentDate:MMM dd, yyyy} has been {newStatus.ToLower()}.",
                            "AppointmentID", appt.AppointmentID);
                    }
                }
                catch (Exception notifyEx)
                {
                    Console.WriteLine($"Notification failed for AppointmentID {appt.AppointmentID}: {notifyEx.Message}");
                }

                return true;
            }
            catch
            {
                await _uow.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> RescheduleAppointmentAsync(AppointmentRescheduleViewModel model, int changedByUserId, int? orgId)
        {
            var appt = await _uow.Appointments.GetByIdAsync(model.AppointmentID);
            if (appt == null) return false;
            if (orgId.HasValue && appt.OrganizationID != orgId.Value) return false;

            if (model.NewDate.Date < DateTime.Today)
            {
                throw new InvalidOperationException("Cannot reschedule to a past date.");
            }

            // 1. Verify availability and leaves
            var dayOfWeek = model.NewDate.ToString("dddd");
            var availabilities = await _uow.DoctorAvailabilities.GetAvailabilityByDoctorIdAsync(appt.DoctorID);
            var availability = availabilities.FirstOrDefault(a => 
                a.DayOfWeek.Equals(dayOfWeek, StringComparison.OrdinalIgnoreCase) && a.IsAvailable);

            if (availability == null)
            {
                throw new InvalidOperationException("Doctor is not available on this day of the week.");
            }

            var leaves = await _uow.DoctorLeaves.GetUpcomingLeavesByDoctorIdAsync(appt.DoctorID);
            var pastLeaves = await _uow.DoctorLeaves.GetPastLeavesByDoctorIdAsync(appt.DoctorID);
            bool onLeave = leaves.Any(l => model.NewDate.Date >= l.LeaveStartDate.Date && model.NewDate.Date <= l.LeaveEndDate.Date)
                        || pastLeaves.Any(l => model.NewDate.Date >= l.LeaveStartDate.Date && model.NewDate.Date <= l.LeaveEndDate.Date);

            if (onLeave)
            {
                throw new InvalidOperationException("Doctor is on leave on the selected date.");
            }

            // 2. Parse slots
            if (!TimeSpan.TryParse(model.NewSlot, out var newStartTime))
            {
                throw new InvalidOperationException("Invalid time slot format.");
            }
            var duration = availability.SlotDuration > 0 ? availability.SlotDuration : 15;
            var newEndTime = newStartTime + TimeSpan.FromMinutes(duration);

            // 3. Break times check
            if (availability.BreakStart.HasValue && availability.BreakEnd.HasValue)
            {
                if (newStartTime < availability.BreakEnd.Value && newEndTime > availability.BreakStart.Value)
                {
                    throw new InvalidOperationException("Selected slot falls during break times.");
                }
            }

            // 4. Double-booking check (excluding this appointment)
            var conflict = await _uow.Appointments.CheckSlotConflictAsync(appt.DoctorID, model.NewDate, newStartTime, newEndTime, appt.AppointmentID);
            if (conflict)
            {
                throw new InvalidOperationException("This time slot is already booked by another patient.");
            }

            var oldStatus = appt.Status;

            // 5. Update inside transaction
            await _uow.BeginTransactionAsync();
            try
            {
                appt.AppointmentDate = model.NewDate;
                appt.StartTime = newStartTime;
                appt.EndTime = newEndTime;
                appt.Status = "Rescheduled";

                var ok = await _uow.Appointments.UpdateAsync(appt);
                if (!ok)
                {
                    await _uow.RollbackAsync();
                    return false;
                }

                var history = new AppointmentStatusHistory
                {
                    AppointmentID = appt.AppointmentID,
                    OldStatus = oldStatus,
                    NewStatus = "Rescheduled",
                    ChangedByUserID = changedByUserId,
                    Remarks = model.Remarks
                };

                await _uow.AppointmentStatusHistories.AddAsync(history);
                await _uow.CommitAsync();
                return true;
            }
            catch
            {
                await _uow.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> SaveConsultationWorkspaceAsync(ConsultationWorkspaceViewModel model, int doctorId, int orgId)
        {
            var appt = await _uow.Appointments.GetByIdAsync(model.AppointmentID);
            if (appt == null || appt.DoctorID != doctorId || appt.OrganizationID != orgId)
            {
                throw new InvalidOperationException("Unauthorized or invalid appointment.");
            }

            var doctor = await _uow.Doctors.GetByIdAsync(doctorId);
            if (doctor == null)
            {
                throw new InvalidOperationException("Doctor not found.");
            }

            if (!appt.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase) && 
                !appt.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Consultation can only be conducted for Approved appointments.");
            }

            // Begin Unit Of Work Transaction
            await _uow.BeginTransactionAsync();
            try
            {
                // 1. Save Doctor Notes
                var existingNote = await _uow.DoctorNotes.GetByAppointmentIdAsync(model.AppointmentID, orgId);
                if (existingNote == null)
                {
                    var note = new DoctorNote
                    {
                        AppointmentID = model.AppointmentID,
                        OrganizationID = orgId,
                        DoctorID = doctorId,
                        PatientID = appt.PatientID,
                        ClinicalNotes = model.ClinicalNotes,
                        Diagnosis = model.Diagnosis,
                        Advice = model.Advice
                    };
                    await _uow.DoctorNotes.AddAsync(note);
                }
                else
                {
                    existingNote.ClinicalNotes = model.ClinicalNotes;
                    existingNote.Diagnosis = model.Diagnosis;
                    existingNote.Advice = model.Advice;
                    await _uow.DoctorNotes.UpdateAsync(existingNote);
                }

                // 2. Save Prescription if enabled
                if (model.CreatePrescription)
                {
                    var existingPresc = await _uow.Prescriptions.GetByAppointmentIdAsync(model.AppointmentID, orgId);
                    if (existingPresc != null)
                    {
                        // Prevent duplicate prescription number generation. Edit existing.
                        existingPresc.GeneralInstructions = model.GeneralInstructions;
                        existingPresc.NextVisitDate = model.NextVisitDate;
                        await _uow.Prescriptions.UpdateAsync(existingPresc);

                        // Clear and recreate prescription medicines
                        await _uow.PrescriptionMedicines.DeleteByPrescriptionIdAsync(existingPresc.PrescriptionID);
                        foreach (var med in model.Medicines)
                        {
                            if (string.IsNullOrWhiteSpace(med.MedicineName)) continue;
                            var pm = new PrescriptionMedicine
                            {
                                PrescriptionID = existingPresc.PrescriptionID,
                                MedicineName = med.MedicineName,
                                Strength = med.Strength,
                                Dosage = med.Dosage,
                                Morning = med.Morning,
                                Afternoon = med.Afternoon,
                                Night = med.Night,
                                BeforeFood = med.BeforeFood,
                                AfterFood = med.AfterFood,
                                DurationDays = med.DurationDays > 0 ? med.DurationDays : 1,
                                Quantity = med.Quantity,
                                Remarks = med.Remarks
                            };
                            await _uow.PrescriptionMedicines.AddAsync(pm);
                        }
                    }
                    else
                    {
                        // Create new
                        var rxNum = await _uow.Prescriptions.GeneratePrescriptionNumberAsync();
                        var presc = new Prescription
                        {
                            AppointmentID = model.AppointmentID,
                            OrganizationID = orgId,
                            DoctorID = doctorId,
                            PatientID = appt.PatientID,
                            PrescriptionNumber = rxNum,
                            GeneralInstructions = model.GeneralInstructions,
                            NextVisitDate = model.NextVisitDate
                        };
                        var prescId = await _uow.Prescriptions.AddAsync(presc);

                        foreach (var med in model.Medicines)
                        {
                            if (string.IsNullOrWhiteSpace(med.MedicineName)) continue;
                            var pm = new PrescriptionMedicine
                            {
                                PrescriptionID = prescId,
                                MedicineName = med.MedicineName,
                                Strength = med.Strength,
                                Dosage = med.Dosage,
                                Morning = med.Morning,
                                Afternoon = med.Afternoon,
                                Night = med.Night,
                                BeforeFood = med.BeforeFood,
                                AfterFood = med.AfterFood,
                                DurationDays = med.DurationDays > 0 ? med.DurationDays : 1,
                                Quantity = med.Quantity,
                                Remarks = med.Remarks
                            };
                            await _uow.PrescriptionMedicines.AddAsync(pm);
                        }
                    }
                }

                // 3. Save Follow-Up if planned
                if (model.NextVisitDate.HasValue)
                {
                    var existingFollowUp = await _uow.FollowUps.GetByAppointmentIdAsync(model.AppointmentID, orgId);
                    if (existingFollowUp == null)
                    {
                        var fu = new FollowUp
                        {
                            AppointmentID = model.AppointmentID,
                            OrganizationID = orgId,
                            DoctorID = doctorId,
                            PatientID = appt.PatientID,
                            FollowUpDate = model.NextVisitDate.Value,
                            Reason = "Consultation follow-up",
                            Status = "Pending"
                        };
                        await _uow.FollowUps.AddAsync(fu);
                    }
                    else
                    {
                        existingFollowUp.FollowUpDate = model.NextVisitDate.Value;
                        await _uow.FollowUps.UpdateAsync(existingFollowUp);
                    }
                }

                // 4. Mark Appointment Completed if it wasn't already
                if (!appt.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                {
                    await _uow.Appointments.UpdateStatusAsync(model.AppointmentID, "Completed");

                    var history = new AppointmentStatusHistory
                    {
                        AppointmentID = model.AppointmentID,
                        OldStatus = appt.Status,
                        NewStatus = "Completed",
                        ChangedByUserID = doctor.UserID,
                        Remarks = "Consultation completed by doctor."
                    };
                    await _uow.AppointmentStatusHistories.AddAsync(history);
                }

                await _uow.CommitAsync();

                // Notification is a best-effort side effect - never surface its failure
                // as a failure of the already-committed consultation record.
                try
                {
                    var patientObj = await _uow.Patients.GetByIdAsync(appt.PatientID);
                    if (patientObj != null)
                    {
                        await _notificationService.SendNotificationAsync(
                            patientObj.UserID, appt.OrganizationID, "Appointment",
                            "Consultation Completed",
                            "Your consultation has been completed. Check your dashboard for prescription and notes.",
                            "AppointmentID", appt.AppointmentID);
                    }
                }
                catch (Exception notifyEx)
                {
                    Console.WriteLine($"Notification failed for AppointmentID {appt.AppointmentID}: {notifyEx.Message}");
                }

                return true;
            }
            catch
            {
                await _uow.RollbackAsync();
                throw;
            }
        }

        private bool IsValidTransition(string currentStatus, string newStatus)
        {
            if (currentStatus.Equals(newStatus, StringComparison.OrdinalIgnoreCase)) return true;

            return currentStatus.ToUpper() switch
            {
                "PENDING" => newStatus.ToUpper() is "APPROVED" or "REJECTED" or "RESCHEDULED" or "CANCELLED",
                "APPROVED" => newStatus.ToUpper() is "COMPLETED" or "NO SHOW" or "CANCELLED" or "RESCHEDULED",
                "RESCHEDULED" => newStatus.ToUpper() is "APPROVED" or "REJECTED" or "CANCELLED",
                "CANCELLED" => false,
                "REJECTED" => false,
                "COMPLETED" => false,
                "NO SHOW" => false,
                _ => false
            };
        }
    }
}
