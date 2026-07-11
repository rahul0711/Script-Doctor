using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharma_Script.Helpers;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.ViewModels.CMS;
using System.Linq;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers
{
    [Authorize(Roles = "Organization Admin")]
    public class FAQsController : Controller
    {
        private readonly IUnitOfWork _uow;

        public FAQsController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        private int OrganizationId => User.GetOrganizationId() ?? 0;

        public async Task<IActionResult> Index()
        {
            var items = await _uow.FAQs.GetByOrganizationIdAsync(OrganizationId);
            return View(items);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_Create", new FAQViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FAQViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Validation failed.", errors = CollectErrors() });
            }

            var faq = new FAQ
            {
                OrganizationID = OrganizationId,
                Question = model.Question,
                Answer = model.Answer,
                DisplayOrder = model.DisplayOrder,
                IsActive = model.IsActive
            };

            await _uow.FAQs.AddAsync(faq);
            return Json(new { success = true, message = "FAQ created." });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var faq = await _uow.FAQs.GetByIdForOrganizationAsync(id, OrganizationId);
            if (faq == null) return NotFound("FAQ not found.");

            var model = new FAQViewModel
            {
                FAQID = faq.FAQID,
                Question = faq.Question,
                Answer = faq.Answer,
                DisplayOrder = faq.DisplayOrder,
                IsActive = faq.IsActive
            };

            return PartialView("_Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FAQViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Validation failed.", errors = CollectErrors() });
            }

            var faq = await _uow.FAQs.GetByIdForOrganizationAsync(model.FAQID, OrganizationId);
            if (faq == null)
            {
                return Json(new { success = false, message = "FAQ not found." });
            }

            faq.Question = model.Question;
            faq.Answer = model.Answer;
            faq.DisplayOrder = model.DisplayOrder;
            faq.IsActive = model.IsActive;

            await _uow.FAQs.UpdateAsync(faq);
            return Json(new { success = true, message = "FAQ updated." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            var result = await _uow.FAQs.SetActiveAsync(id, OrganizationId, isActive);
            if (result)
            {
                return Json(new { success = true, message = $"FAQ {(isActive ? "activated" : "deactivated")}." });
            }
            return Json(new { success = false, message = "FAQ not found." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var faq = await _uow.FAQs.GetByIdForOrganizationAsync(id, OrganizationId);
            if (faq == null)
            {
                return Json(new { success = false, message = "FAQ not found." });
            }

            var result = await _uow.FAQs.DeleteForOrganizationAsync(id, OrganizationId);
            if (result)
            {
                return Json(new { success = true, message = "FAQ deleted." });
            }
            return Json(new { success = false, message = "FAQ could not be deleted." });
        }

        private string[] CollectErrors()
        {
            return ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
        }
    }
}
