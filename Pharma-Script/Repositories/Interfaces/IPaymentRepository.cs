using Pharma_Script.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<Payment?> GetByAppointmentIdAsync(int appointmentId, int? orgId);
        Task<IEnumerable<Payment>> SearchAndPaginateAsync(int? orgId, string? status, string? searchTerm, int page, int pageSize);
        Task<int> GetSearchCountAsync(int? orgId, string? status, string? searchTerm);
    }
}
