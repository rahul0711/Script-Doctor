using Pharma_Script.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IHeroSectionRepository : IRepository<HeroSection>
    {
        Task<IEnumerable<HeroSection>> GetByOrganizationIdAsync(int organizationId);
        Task<IEnumerable<HeroSection>> GetActiveByOrganizationIdAsync(int organizationId);
        Task<HeroSection?> GetByIdForOrganizationAsync(int id, int organizationId);
        Task<bool> SetActiveAsync(int id, int organizationId, bool isActive);
        Task<bool> DeleteForOrganizationAsync(int id, int organizationId);
    }
}
