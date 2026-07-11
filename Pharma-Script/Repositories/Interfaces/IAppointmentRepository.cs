using Pharma_Script.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        Task<IEnumerable<Appointment>> SearchAndPaginateAsync(int? orgId, int? branchId, int? doctorId, int? patientId, string? status, string? type, DateTime? startDate, DateTime? endDate, bool? isPriority, string? searchTerm, int page, int pageSize);
        Task<int> GetSearchCountAsync(int? orgId, int? branchId, int? doctorId, int? patientId, string? status, string? type, DateTime? startDate, DateTime? endDate, bool? isPriority, string? searchTerm);
        Task<bool> UpdateStatusAsync(int appointmentId, string status);
        Task<IEnumerable<Appointment>> GetBookedSlotsAsync(int doctorId, DateTime date);
        Task<bool> CheckSlotConflictAsync(int doctorId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeAppointmentId);
        Task<Appointment?> GetAppointmentDetailsByIdAsync(int id, int? orgId);
    }
}
