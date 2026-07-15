using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharma_Script.Helpers;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.ViewModels.Public;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers.Public
{
    [AllowAnonymous]
    [Route("{slug:activeOrgSlug}/login")]
    public class PublicLoginController : PublicControllerBase
    {
        public PublicLoginController(IUnitOfWork uow) : base(uow)
        {
        }

        // GET /{slug}/login
        [HttpGet("")]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            // If already authenticated as a Patient belonging to this org, go home.
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Patient"))
            {
                var orgIdClaim = User.GetOrganizationId();
                if (orgIdClaim.HasValue && orgIdClaim.Value == OrganizationId)
                {
                    return RedirectToLocalOrHome(returnUrl);
                }
            }

            ViewData["ReturnUrl"] = returnUrl;
            ViewData["Title"] = "Patient Login";
            return View(new PublicPatientLoginViewModel());
        }

        // POST /{slug}/login
        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(PublicPatientLoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Find user by email
            var user = await Uow.Users.GetByEmailAsync(model.Email);
            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            // 2. Verify password
            if (!PasswordHasher.VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            // 3. Must be a Patient role
            if (!string.Equals(user.RoleName, "Patient", System.StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "This login is for patients only. Staff should use the admin portal.");
                return View(model);
            }

            // 4. Patient record must belong to THIS organization (slug-resolved, never client-supplied)
            var patient = await Uow.Patients.GetByUserIdAsync(user.UserID);
            if (patient == null || patient.OrganizationID != OrganizationId)
            {
                ModelState.AddModelError(string.Empty,
                    "Your account is not registered with this clinic. Please register here or visit your clinic's website.");
                return View(model);
            }

            // 5. Sign in
            await SignInPatientAsync(user, model.RememberMe);
            return RedirectToLocalOrHome(returnUrl);
        }

        // POST /{slug}/logout  (also accepts GET for convenience links)
        [HttpPost("logout")]
        [HttpGet("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "PublicHome", new { slug = Tenant.Organization.OrganizationSlug });
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private async Task SignInPatientAsync(Models.User user, bool rememberMe)
        {
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
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = System.DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                authProperties);
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
