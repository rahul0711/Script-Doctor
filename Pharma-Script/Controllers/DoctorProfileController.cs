using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pharma_Script.Helpers;
using Pharma_Script.ViewModels.Doctor;
using Pharma_Script.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers
{
    // Doctor's own self-service profile — distinct from DoctorsController, which is
    // admin-only. A doctor may edit their personal/practice info here, but never
    // Organization, Branch, Department, IsActive, or the Razorpay payment gateway.
    [Authorize(Roles = "Doctor")]
    public class DoctorProfileController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _env;

        public DoctorProfileController(IUnitOfWork uow, IWebHostEnvironment env)
        {
            _uow = uow;
            _env = env;
        }

        // GET: /DoctorProfile
        public async Task<IActionResult> Index()
        {
            var userId = User.GetUserId();
            var doctor = await _uow.Doctors.GetByUserIdAsync(userId);
            if (doctor == null)
            {
                TempData["Error"] = "Doctor record not found.";
                return RedirectToAction("Index", "Dashboard");
            }

            var doc = await _uow.Doctors.GetDoctorDetailsByIdAsync(doctor.DoctorID, doctor.OrganizationID);
            if (doc == null)
            {
                TempData["Error"] = "Doctor profile not found.";
                return RedirectToAction("Index", "Dashboard");
            }

            var model = new DoctorProfileViewModel
            {
                DoctorID = doc.DoctorID,
                UserID = doc.UserID,
                ExistingProfileImage = doc.ProfileImage,
                FirstName = doc.FirstName,
                LastName = doc.LastName,
                Email = doc.Email,
                Phone = doc.Phone,
                Qualification = doc.Qualification,
                ExperienceYears = doc.ExperienceYears,
                MedicalRegistrationNumber = doc.MedicalRegistrationNumber,
                Biography = doc.Biography,
                ConsultationFee = doc.ConsultationFee,
                VideoConsultationFee = doc.VideoConsultationFee,
                VoiceConsultationFee = doc.VoiceConsultationFee,
                SpecializationIDs = doc.SpecializationIDs
            };

            var specs = await _uow.Specializations.GetAllAsync();
            ViewBag.Specializations = specs.Where(s => s.IsActive == true).ToList();

            return View(model);
        }

        // POST: /DoctorProfile/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(DoctorProfileViewModel model)
        {
            var userId = User.GetUserId();
            var doctor = await _uow.Doctors.GetByUserIdAsync(userId);
            if (doctor == null || doctor.DoctorID != model.DoctorID)
            {
                TempData["Error"] = "Doctor profile not found.";
                return RedirectToAction("Index");
            }

            ModelState.Remove("CurrentPassword");
            ModelState.Remove("NewPassword");
            ModelState.Remove("ConfirmNewPassword");

            var wantsPasswordChange = !string.IsNullOrWhiteSpace(model.NewPassword);

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the highlighted errors and try again.";
                var specs = await _uow.Specializations.GetAllAsync();
                ViewBag.Specializations = specs.Where(s => s.IsActive == true).ToList();
                return View("Index", model);
            }

            var user = await _uow.Users.GetByIdAsync(model.UserID);
            if (user == null)
            {
                TempData["Error"] = "User account not found.";
                return RedirectToAction("Index");
            }

            // Duplicate email check (excluding self)
            var existingUser = await _uow.Users.GetByEmailAsync(model.Email);
            if (existingUser != null && existingUser.UserID != model.UserID)
            {
                TempData["Error"] = "Email address is already in use by another user.";
                return RedirectToAction("Index");
            }

            if (wantsPasswordChange)
            {
                if (string.IsNullOrWhiteSpace(model.CurrentPassword) || !PasswordHasher.VerifyPassword(model.CurrentPassword, user.PasswordHash))
                {
                    TempData["Error"] = "Current password is incorrect. Password was not changed.";
                    return RedirectToAction("Index");
                }
            }

            await _uow.BeginTransactionAsync();
            try
            {
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.Phone = model.Phone;

                if (model.ProfileImageFile != null)
                {
                    var newProfileImage = await CmsImageUploadHelper.UploadAsync(model.ProfileImageFile, _env.WebRootPath, doctor.OrganizationID, "doctors", 3 * 1024 * 1024);
                    CmsImageUploadHelper.DeleteIfExists(_env.WebRootPath, user.ProfileImage);
                    user.ProfileImage = newProfileImage;
                }

                if (wantsPasswordChange)
                {
                    user.PasswordHash = PasswordHasher.HashPassword(model.NewPassword!);
                }

                await _uow.Users.UpdateAsync(user);

                doctor.Qualification = model.Qualification;
                doctor.ExperienceYears = model.ExperienceYears;
                doctor.MedicalRegistrationNumber = model.MedicalRegistrationNumber;
                doctor.Biography = model.Biography;
                doctor.ConsultationFee = model.ConsultationFee;
                doctor.VideoConsultationFee = model.VideoConsultationFee;
                doctor.VoiceConsultationFee = model.VoiceConsultationFee;

                await _uow.Doctors.UpdateAsync(doctor);

                await _uow.Doctors.ClearDoctorSpecializationsAsync(doctor.DoctorID);
                await _uow.Doctors.AddDoctorSpecializationsAsync(doctor.DoctorID, model.SpecializationIDs);

                await _uow.CommitAsync();

                // Refresh the auth cookie so the navbar name/initials reflect the change immediately.
                await ReissueAuthCookieAsync(user);

                TempData["Success"] = wantsPasswordChange
                    ? "Profile and password updated successfully."
                    : "Profile updated successfully.";
                return RedirectToAction("Index");
            }
            catch (CmsUploadValidationException ex)
            {
                await _uow.RollbackAsync();
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        private async Task ReissueAuthCookieAsync(Models.User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, "Doctor"),
                new Claim("FullName", $"{user.FirstName} {user.LastName}".Trim())
            };
            if (user.OrganizationID.HasValue)
            {
                claims.Add(new Claim("OrganizationID", user.OrganizationID.Value.ToString()));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var currentAuth = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = currentAuth.Properties ?? new AuthenticationProperties { ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2) };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), authProperties);
        }
    }
}
