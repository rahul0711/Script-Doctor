using Pharma_Script.Models;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IRoleRepository : IRepository<Role>
    {
        Task<Role?> GetByNameAsync(string roleName);
    }
}
