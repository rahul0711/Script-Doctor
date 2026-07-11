using Pharma_Script.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IFAQRepository : IRepository<FAQ>
    {
        Task<IEnumerable<FAQ>> GetByOrganizationIdAsync(int organizationId);
        Task<IEnumerable<FAQ>> GetActiveByOrganizationIdAsync(int organizationId);
        Task<FAQ?> GetByIdForOrganizationAsync(int id, int organizationId);
        Task<bool> SetActiveAsync(int id, int organizationId, bool isActive);
        Task<bool> UpdateDisplayOrderAsync(int id, int organizationId, int displayOrder);
        Task<bool> DeleteForOrganizationAsync(int id, int organizationId);
    }
}
