using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.Services.Interfaces;

namespace Pharma_Script.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _uow;

        public NotificationService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(int userId, int organizationId)
        {
            return await _uow.Notifications.GetUnreadByUserIdAsync(userId, organizationId);
        }

        public async Task<IEnumerable<Notification>> GetAllNotificationsAsync(int userId, int organizationId, int limit = 50)
        {
            return await _uow.Notifications.GetAllByUserIdAsync(userId, organizationId, limit);
        }

        public async Task<bool> SendNotificationAsync(int userId, int organizationId, string type, string title, string message, string? relatedEntityType = null, int? relatedEntityId = null)
        {
            var notification = new Notification
            {
                OrganizationID = organizationId,
                UserID = userId,
                NotificationType = type,
                Title = title,
                Message = message,
                RelatedEntityType = relatedEntityType,
                RelatedEntityID = relatedEntityId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.Notifications.CreateAsync(notification);
            return true;
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId, int organizationId)
        {
            return await _uow.Notifications.MarkAsReadAsync(notificationId, userId, organizationId);
        }

        public async Task<bool> MarkAllAsReadAsync(int userId, int organizationId)
        {
            return await _uow.Notifications.MarkAllAsReadAsync(userId, organizationId);
        }
    }
}
