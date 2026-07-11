using Pharma_Script.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IDepartmentRepository : IRepository<Department>
    {
        Task<IEnumerable<Department>> GetByOrganizationIdAsync(int organizationId);
    }
}
