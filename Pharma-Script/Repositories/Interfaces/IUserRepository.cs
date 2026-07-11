using Pharma_Script.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetByOrganizationIdAsync(int organizationId);
        Task<bool> UpdateStatusAsync(int id, bool isActive);
        Task<bool> UpdatePasswordAsync(int id, string passwordHash);
    }
}
