using System.Collections.Generic;
using System.Threading.Tasks;
using Pharma_Script.Models;

namespace Pharma_Script.Services.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(int userId, int organizationId);
        Task<IEnumerable<Notification>> GetAllNotificationsAsync(int userId, int organizationId, int limit = 50);
        Task<bool> SendNotificationAsync(int userId, int organizationId, string type, string title, string message, string? relatedEntityType = null, int? relatedEntityId = null);
        Task<bool> MarkAsReadAsync(int notificationId, int userId, int organizationId);
        Task<bool> MarkAllAsReadAsync(int userId, int organizationId);
    }
}
