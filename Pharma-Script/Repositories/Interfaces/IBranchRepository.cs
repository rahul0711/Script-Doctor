using Pharma_Script.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IBranchRepository : IRepository<Branch>
    {
        Task<IEnumerable<Branch>> GetByOrganizationIdAsync(int organizationId);
    }
}
