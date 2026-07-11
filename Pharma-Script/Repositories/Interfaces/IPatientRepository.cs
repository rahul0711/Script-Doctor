using Pharma_Script.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IPatientRepository : IRepository<Patient>
    {
        Task<IEnumerable<Patient>> SearchAndPaginateAsync(int? orgId, int? branchId, string searchTerm, int page, int pageSize);
        Task<int> GetSearchCountAsync(int? orgId, int? branchId, string searchTerm);
        Task<bool> UpdateStatusAsync(int id, bool isActive);
        Task<Patient?> GetPatientDetailsByIdAsync(int id, int? orgId);
        Task<Patient?> GetByUserIdAsync(int userId);
    }
}
