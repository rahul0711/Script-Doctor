using Pharma_Script.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IOrganizationRepository : IRepository<Organization>
    {
        Task<IEnumerable<Organization>> SearchAndPaginateAsync(string searchTerm, int page, int pageSize);
        Task<int> GetSearchCountAsync(string searchTerm);
        Task<bool> UpdateStatusAsync(int id, bool isActive);
        Task<Organization?> GetBySlugAsync(string slug);
        Task<bool> IsSlugTakenAsync(string slug, int? excludeOrganizationId);
    }
}
