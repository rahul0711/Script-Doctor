using System.Collections.Generic;
using System.Threading.Tasks;
using Pharma_Script.Models;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(int userId, int organizationId);
        Task<IEnumerable<Notification>> GetAllByUserIdAsync(int userId, int organizationId, int limit = 50);
        Task<int> CreateAsync(Notification notification);
        Task<bool> MarkAsReadAsync(int notificationId, int userId, int organizationId);
        Task<bool> MarkAllAsReadAsync(int userId, int organizationId);
    }
}
