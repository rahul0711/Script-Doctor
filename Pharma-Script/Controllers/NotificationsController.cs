using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharma_Script.Helpers;
using Pharma_Script.Services.Interfaces;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers
{
    [Authorize]
    [Route("Notifications")]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("Unread")]
        public async Task<IActionResult> GetUnread()
        {
            var userId = User.GetUserId();
            var orgId = User.GetOrganizationId() ?? 0;
            
            var notifications = await _notificationService.GetUnreadNotificationsAsync(userId, orgId);
            return Json(notifications);
        }

        [HttpPost("MarkAsRead/{id:int}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.GetUserId();
            var orgId = User.GetOrganizationId() ?? 0;

            var success = await _notificationService.MarkAsReadAsync(id, userId, orgId);
            return Json(new { success });
        }

        [HttpPost("MarkAllAsRead")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.GetUserId();
            var orgId = User.GetOrganizationId() ?? 0;

            var success = await _notificationService.MarkAllAsReadAsync(userId, orgId);
            return Json(new { success });
        }

        [HttpGet("All")]
        public async Task<IActionResult> All()
        {
            var userId = User.GetUserId();
            var orgId = User.GetOrganizationId() ?? 0;
            
            var notifications = await _notificationService.GetAllNotificationsAsync(userId, orgId);
            
            ViewData["Title"] = "All Notifications";
            return View(notifications);
        }
    }
}
