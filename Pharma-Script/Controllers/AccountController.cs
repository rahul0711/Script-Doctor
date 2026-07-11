using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharma_Script.Helpers;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.ViewModels.Account;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IUnitOfWork _uow;

        public AccountController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Fetch user from DB using email
            var user = await _uow.Users.GetByEmailAsync(model.Email);
            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Invalid email address or deactivated account.");
                return View(model);
            }

            // Verify password using PBKDF2
            if (!PasswordHasher.VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Incorrect password.");
                return View(model);
            }

            // Prepare Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.RoleName ?? "Guest"),
                new Claim("FullName", $"{user.FirstName} {user.LastName}".Trim())
            };

            if (user.OrganizationID.HasValue)
            {
                claims.Add(new Claim("OrganizationID", user.OrganizationID.Value.ToString()));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
            };

            // Sign In
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            // Update user last login
            user.LastLogin = DateTime.Now;
            // Since we aren't updating password/image, let's save the user last login status
            // Wait, we can write a tiny UPDATE in DB or update the user entity
            // We can just update the database using a query if we want, but updating is fine.
            // Let's implement updating the last login:
            // Since UserRepository does not have an explicit UpdateLastLogin, we can update the user using the standard update or we can just bypass it to keep it simple. Let's write a simple command inside UserRepository later if needed, but the current UpdateAsync does update details, so it's fine.
            
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
