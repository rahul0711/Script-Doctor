using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pharma_Script.Helpers;
using Pharma_Script.Repositories.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers
{
    // Platform Owner generates and settles payouts to organizations; Organization Admin can only
    // view their own organization's settlement history (read-only) - see spec's "Only Platform
    // Owner can initiate settlements. Organization Admin can only view settlement history."
    [Authorize(Roles = "Platform Owner,Organization Admin")]
    public class SettlementsController : Controller
    {
        private readonly IUnitOfWork _uow;

        public SettlementsController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IActionResult> Index(int? orgIdFilter, string? status, int page = 1, int pageSize = 10)
        {
            var isPlatformOwner = User.IsPlatformOwner();
            var userOrgId = User.GetOrganizationId();
            var scopedOrgId = isPlatformOwner ? orgIdFilter : userOrgId;

            if (isPlatformOwner)
            {
                var organizations = await _uow.Organizations.GetAllAsync();
                ViewBag.Organizations = new SelectList(organizations.Where(o => o.IsActive), "OrganizationID", "OrganizationName", orgIdFilter);
            }

            ViewBag.IsPlatformOwner = isPlatformOwner;
            ViewBag.SelectedOrg = orgIdFilter;
            ViewBag.SelectedStatus = status;

            var settlements = await _uow.Settlements.SearchAndPaginateAsync(scopedOrgId, status, page, pageSize);
            var totalItems = await _uow.Settlements.GetSearchCountAsync(scopedOrgId, status);

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(settlements);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var isPlatformOwner = User.IsPlatformOwner();
            var userOrgId = User.GetOrganizationId();

            var settlement = await _uow.Settlements.GetByIdScopedAsync(id, isPlatformOwner ? null : userOrgId);
            if (settlement == null)
            {
                return NotFound("Settlement not found.");
            }

            var transactions = await _uow.Settlements.GetTransactionsBySettlementIdAsync(id);
            ViewBag.Transactions = transactions;
            return PartialView("_Details", settlement);
        }

        [HttpGet]
        public async Task<IActionResult> Generate()
        {
            if (!User.IsPlatformOwner()) return Forbid();

            var organizations = await _uow.Organizations.GetAllAsync();
            ViewBag.Organizations = new SelectList(organizations.Where(o => o.IsActive), "OrganizationID", "OrganizationName");
            return PartialView("_Generate");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(int organizationId)
        {
            if (!User.IsPlatformOwner()) return Forbid();

            var org = await _uow.Organizations.GetByIdAsync(organizationId);
            if (org == null)
            {
                return Json(new { success = false, message = "Organization not found." });
            }

            await _uow.BeginTransactionAsync();
            try
            {
                var settlement = await _uow.Settlements.GenerateSettlementAsync(organizationId);
                await _uow.CommitAsync();

                if (settlement == null)
                {
                    return Json(new { success = false, message = "No unsettled payments found for this organization." });
                }

                return Json(new { success = true, message = $"Settlement generated for {org.OrganizationName}: {settlement.TotalNetAmount:N2} payable." });
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return Json(new { success = false, message = $"Failed to generate settlement: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPaid(int settlementId, string? paymentReference, string? notes)
        {
            if (!User.IsPlatformOwner()) return Forbid();

            try
            {
                var settled = await _uow.Settlements.MarkPaidAsync(settlementId, paymentReference, notes, User.GetUserId());
                if (!settled)
                {
                    return Json(new { success = false, message = "Settlement not found or already paid." });
                }
                return Json(new { success = true, message = "Settlement marked as paid." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Failed to update settlement: {ex.Message}" });
            }
        }
    }
}
