using Pharma_Script.Models;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface ICMSSettingRepository : IRepository<CMSSetting>
    {
        Task<CMSSetting?> GetByOrganizationIdAsync(int organizationId);
        Task<int> UpsertAsync(CMSSetting entity);
    }
}
