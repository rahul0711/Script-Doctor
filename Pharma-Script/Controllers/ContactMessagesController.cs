using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharma_Script.Helpers;
using Pharma_Script.Repositories.Interfaces;
using System;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers
{
    [Authorize(Roles = "Organization Admin")]
    public class ContactMessagesController : Controller
    {
        private readonly IUnitOfWork _uow;

        public ContactMessagesController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        private int OrganizationId => User.GetOrganizationId() ?? 0;

        public async Task<IActionResult> Index(string filter = "all", int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            bool? isRead = filter switch
            {
                "read" => true,
                "unread" => false,
                _ => null
            };

            var items = await _uow.ContactMessages.SearchAndPaginateAsync(OrganizationId, isRead, page, pageSize);
            var totalItems = await _uow.ContactMessages.GetSearchCountAsync(OrganizationId, isRead);

            ViewBag.Filter = filter;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.UnreadCount = await _uow.ContactMessages.GetUnreadCountAsync(OrganizationId);

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var message = await _uow.ContactMessages.GetByIdForOrganizationAsync(id, OrganizationId);
            if (message == null) return NotFound("Message not found.");

            if (!message.IsRead)
            {
                await _uow.ContactMessages.SetReadAsync(id, OrganizationId, true);
                message.IsRead = true;
            }

            return PartialView("_Details", message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetRead(int id, bool isRead)
        {
            var result = await _uow.ContactMessages.SetReadAsync(id, OrganizationId, isRead);
            if (result)
            {
                return Json(new { success = true, message = $"Message marked as {(isRead ? "read" : "unread")}." });
            }
            return Json(new { success = false, message = "Message not found." });
        }
    }
}
