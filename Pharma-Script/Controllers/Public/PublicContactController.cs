using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.ViewModels.Public;
using System.Linq;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers.Public
{
    [AllowAnonymous]
    [Route("{slug:activeOrgSlug}/contact")]
    public class PublicContactController : PublicControllerBase
    {
        public PublicContactController(IUnitOfWork uow) : base(uow)
        {
        }

        [HttpPost("submit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(PublicContactFormViewModel model)
        {
            // Honeypot: bots fill every field, real visitors never see "Website".
            if (!string.IsNullOrWhiteSpace(model.Website))
            {
                return Json(new { success = true, message = "Thank you. Your message has been sent." });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Please check the form and try again.", errors });
            }

            var message = new ContactMessage
            {
                OrganizationID = OrganizationId,
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                Subject = model.Subject,
                Message = model.Message
            };

            await Uow.ContactMessages.AddAsync(message);
            return Json(new { success = true, message = "Thank you. Your message has been sent." });
        }
    }
}
