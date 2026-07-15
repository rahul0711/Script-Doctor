using System;
using System.Threading.Tasks;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.Services.Interfaces;

namespace Pharma_Script.Services.Implementations
{
    public class ConsultationSessionService : IConsultationSessionService
    {
        private readonly IUnitOfWork _uow;

        public ConsultationSessionService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<ConsultationSession> GetOrCreateSessionAsync(int appointmentId, int organizationId)
        {
            var session = await _uow.ConsultationSessions.GetByAppointmentIdAsync(appointmentId, organizationId);
            if (session != null) return session;

            var appt = await _uow.Appointments.GetAppointmentDetailsByIdAsync(appointmentId, organizationId);
            if (appt == null) throw new Exception("Appointment not found");

            var newSession = new ConsultationSession
            {
                OrganizationID = appt.OrganizationID,
                AppointmentID = appt.AppointmentID,
                DoctorID = appt.DoctorID,
                PatientID = appt.PatientID,
                ConsultationType = appt.AppointmentType,
                SessionStatus = "Pending",
                CreatedByUserID = appt.DoctorID // Default to doctor, or could be system
            };

            await _uow.ConsultationSessions.CreateAsync(newSession);
            return newSession;
        }

        public async Task<bool> UpdateVideoLinkAsync(int appointmentId, string provider, string url, int organizationId)
        {
            var session = await GetOrCreateSessionAsync(appointmentId, organizationId);
            
            session.MeetingProvider = provider;
            session.MeetingURL = url;
            if (session.SessionStatus == "Pending")
            {
                session.SessionStatus = "Ready";
            }

            return await _uow.ConsultationSessions.UpdateAsync(session);
        }

        public async Task<bool> UpdateSessionStatusAsync(int appointmentId, string status, int organizationId)
        {
            var session = await GetOrCreateSessionAsync(appointmentId, organizationId);
            session.SessionStatus = status;
            return await _uow.ConsultationSessions.UpdateAsync(session);
        }
    }
}
