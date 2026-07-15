using System;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.Services.Interfaces;

namespace Pharma_Script.Services.Implementations
{
    public class ReminderService : IReminderService
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notificationService;

        public ReminderService(IUnitOfWork uow, INotificationService notificationService)
        {
            _uow = uow;
            _notificationService = notificationService;
        }

        public async Task<int> GenerateDailyRemindersAsync()
        {
            int createdCount = 0;
            var tomorrow = DateTime.Today.AddDays(1);

            var appointments = await _uow.Appointments.GetUpcomingAppointmentsForRemindersAsync(tomorrow);

            foreach (var appt in appointments)
            {
                // Send reminder to Patient
                var patient = await _uow.Patients.GetByIdAsync(appt.PatientID);
                if (patient != null)
                {
                    await _notificationService.SendNotificationAsync(
                        patient.UserID, appt.OrganizationID, "Reminder",
                        "Upcoming Appointment",
                        $"You have an appointment tomorrow at {appt.StartTime:hh\\:mm} with your doctor.",
                        "AppointmentID", appt.AppointmentID);
                    createdCount++;
                }
                
                // Send reminder to Doctor
                var doctor = await _uow.Doctors.GetByIdAsync(appt.DoctorID);
                if (doctor != null)
                {
                    await _notificationService.SendNotificationAsync(
                        doctor.UserID, appt.OrganizationID, "Reminder",
                        "Upcoming Appointment",
                        $"You have an appointment tomorrow at {appt.StartTime:hh\\:mm} with {patient?.FirstName} {patient?.LastName}.",
                        "AppointmentID", appt.AppointmentID);
                    createdCount++;
                }
            }

            return createdCount;
        }
    }
}
