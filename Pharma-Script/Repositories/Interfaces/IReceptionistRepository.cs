using Pharma_Script.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IReceptionistRepository : IRepository<Receptionist>
    {
        Task<IEnumerable<Receptionist>> SearchAndPaginateAsync(int? orgId, int? branchId, string searchTerm, int page, int pageSize);
        Task<int> GetSearchCountAsync(int? orgId, int? branchId, string searchTerm);
        Task<bool> UpdateStatusAsync(int id, bool isActive);
        Task<Receptionist?> GetByUserIdAsync(int userId);
    }
}
