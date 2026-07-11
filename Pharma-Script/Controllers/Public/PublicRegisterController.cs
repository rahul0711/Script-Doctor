using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.Services.Interfaces;
using Pharma_Script.ViewModels.Public;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers.Public
{
    [AllowAnonymous]
    [Route("{slug:activeOrgSlug}/register")]
    public class PublicRegisterController : PublicControllerBase
    {
        private readonly IPatientProvisioningService _patientProvisioningService;

        public PublicRegisterController(IUnitOfWork uow, IPatientProvisioningService patientProvisioningService) : base(uow)
        {
            _patientProvisioningService = patientProvisioningService;
        }

        [HttpGet("")]
        public IActionResult Register(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToLocalOrHome(returnUrl);
            }

            ViewData["ReturnUrl"] = returnUrl;
            ViewData["Title"] = "Create Account";
            return View(new PublicPatientRegisterViewModel());
        }

        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(PublicPatientRegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var patient = await _patientProvisioningService.CreatePatientAsync(new PatientProvisioningRequest
                {
                    OrganizationID = OrganizationId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Password = model.Password,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    IsActive = true
                });

                await SignInPatientAsync(patient.UserID);
                return RedirectToLocalOrHome(returnUrl);
            }
            catch (PatientProvisioningException)
            {
                // Do not reveal whether the email specifically exists - point to sign in instead.
                ModelState.AddModelError(string.Empty, "An account may already exist with these details. Please sign in instead.");
                return View(model);
            }
        }

        private async Task SignInPatientAsync(int userId)
        {
            var user = await Uow.Users.GetByIdAsync(userId);
            if (user == null) return;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, "Patient"),
                new Claim("FullName", $"{user.FirstName} {user.LastName}".Trim())
            };

            if (user.OrganizationID.HasValue)
            {
                claims.Add(new Claim("OrganizationID", user.OrganizationID.Value.ToString()));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        }

        private IActionResult RedirectToLocalOrHome(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "PublicHome", new { slug = Tenant.Organization.OrganizationSlug });
        }
    }
}
