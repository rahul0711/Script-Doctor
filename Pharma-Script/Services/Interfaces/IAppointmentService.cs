using Pharma_Script.Models;
using Pharma_Script.ViewModels.Appointment;
using Pharma_Script.ViewModels.Consultation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pharma_Script.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<IEnumerable<string>> GetAvailableSlotsAsync(int doctorId, DateTime date);
        Task<decimal> QuoteFeeAsync(AppointmentBookingViewModel model, int orgId);
        Task<Appointment> BookAppointmentAsync(AppointmentBookingViewModel model, int orgId, int? branchId, Payment? payment = null);
        Task<bool> UpdateAppointmentStatusAsync(int appointmentId, string newStatus, int changedByUserId, string? remarks, int? orgId);
        Task<bool> RescheduleAppointmentAsync(AppointmentRescheduleViewModel model, int changedByUserId, int? orgId);
        Task<bool> SaveConsultationWorkspaceAsync(ConsultationWorkspaceViewModel model, int doctorId, int orgId);
    }
}
