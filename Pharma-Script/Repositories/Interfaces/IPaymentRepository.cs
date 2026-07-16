using Pharma_Script.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<Payment?> GetByAppointmentIdAsync(int appointmentId, int? orgId);
        Task<Payment?> GetByTransactionReferenceAsync(string transactionReference);
        Task<IEnumerable<Payment>> SearchAndPaginateAsync(int? orgId, string? status, string? searchTerm, int page, int pageSize);
        Task<int> GetSearchCountAsync(int? orgId, string? status, string? searchTerm);
        Task<decimal> GetTotalByDoctorIdAsync(int doctorId, int? orgId);
        Task<decimal> GetTotalByOrgIdAsync(int orgId);
        Task<IEnumerable<Payment>> GetByPatientAndOrgAsync(int patientId, int orgId);
    }
}
