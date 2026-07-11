using Pharma_Script.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IContactMessageRepository : IRepository<ContactMessage>
    {
        Task<IEnumerable<ContactMessage>> SearchAndPaginateAsync(int organizationId, bool? isRead, int page, int pageSize);
        Task<int> GetSearchCountAsync(int organizationId, bool? isRead);
        Task<ContactMessage?> GetByIdForOrganizationAsync(int id, int organizationId);
        Task<bool> SetReadAsync(int id, int organizationId, bool isRead);
        Task<int> GetUnreadCountAsync(int organizationId);
    }
}
