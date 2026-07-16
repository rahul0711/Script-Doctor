using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pharma_Script.Helpers;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.ViewModels.Doctor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers
{
    [Authorize(Roles = "Platform Owner,Organization Admin")]
    public class DoctorsController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _env;

        public DoctorsController(IUnitOfWork uow, IWebHostEnvironment env)
        {
            _uow = uow;
            _env = env;
        }

        // GET: /Doctors
        public async Task<IActionResult> Index(int? orgIdFilter, int? branchId, int? departmentId, int? specializationId, bool? isActive, string searchTerm, int page = 1, int pageSize = 10)
        {
            var isPlatformOwner = User.IsPlatformOwner();
            var userOrgId = User.GetOrganizationId();
            var activeOrgId = isPlatformOwner ? orgIdFilter : userOrgId;

            // Load filters metadata
            IEnumerable<Branch> branches;
            IEnumerable<Department> departments;
            if (isPlatformOwner)
            {
                var organizations = await _uow.Organizations.GetAllAsync();
                ViewBag.Organizations = new SelectList(organizations.Where(o => o.IsActive), "OrganizationID", "OrganizationName", orgIdFilter);
                
                branches = activeOrgId.HasValue ? await _uow.Branches.GetByOrganizationIdAsync(activeOrgId.Value) : await _uow.Branches.GetAllAsync();
                departments = activeOrgId.HasValue ? await _uow.Departments.GetByOrganizationIdAsync(activeOrgId.Value) : await _uow.Departments.GetAllAsync();
            }
            else if (userOrgId.HasValue)
            {
                branches = await _uow.Branches.GetByOrganizationIdAsync(userOrgId.Value);
                departments = await _uow.Departments.GetByOrganizationIdAsync(userOrgId.Value);
            }
            else
            {
                branches = new List<Branch>();
                departments = new List<Department>();
            }

            var specializations = await _uow.Specializations.GetAllAsync();

            ViewBag.Branches = new SelectList(branches.Where(b => b.IsActive == true), "BranchID", "BranchName", branchId);
            ViewBag.Departments = new SelectList(departments.Where(d => d.IsActive == true), "DepartmentID", "DepartmentName", departmentId);
            ViewBag.Specializations = new SelectList(specializations.Where(s => s.IsActive == true), "SpecializationID", "SpecializationName", specializationId);
            ViewBag.ActiveStatusList = new SelectList(new[]
            {
                new { Value = "true", Text = "Active Only" },
                new { Value = "false", Text = "Inactive Only" }
            }, "Value", "Text", isActive?.ToString().ToLower());

            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedOrg = orgIdFilter;
            ViewBag.SelectedBranch = branchId;
            ViewBag.SelectedDepartment = departmentId;
            ViewBag.SelectedSpecialization = specializationId;
            ViewBag.SelectedStatus = isActive;

            // Fetch and Paginate
            var doctors = await _uow.Doctors.SearchAndPaginateAsync(activeOrgId, branchId, departmentId, specializationId, isActive, searchTerm, page, pageSize);
            var totalItems = await _uow.Doctors.GetSearchCountAsync(activeOrgId, branchId, departmentId, specializationId, isActive, searchTerm);

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(doctors);
        }

        // GET: /Doctors/Details/5 (Partial View for Modal)
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userOrgId = User.GetOrganizationId();
            var doc = await _uow.Doctors.GetDoctorDetailsByIdAsync(id, User.IsPlatformOwner() ? null : userOrgId);
            if (doc == null)
            {
                return NotFound("Doctor profile not found.");
            }

            return PartialView("_Details", doc);
        }

        // GET: /Doctors/StatsPanel/5 (Partial view – doctor patient & revenue stats)
        [HttpGet]
        public async Task<IActionResult> StatsPanel(int id)
        {
            var userOrgId = User.GetOrganizationId();
            var isPlatformOwner = User.IsPlatformOwner();

            var doc = await _uow.Doctors.GetDoctorDetailsByIdAsync(id, isPlatformOwner ? null : userOrgId);
            if (doc == null)
                return NotFound("Doctor profile not found.");

            var appointments = (await _uow.Appointments.GetByDoctorIdAsync(id, isPlatformOwner ? null : userOrgId)).ToList();
            var totalRevenue = await _uow.Payments.GetTotalByDoctorIdAsync(id, isPlatformOwner ? null : userOrgId);

            var uniquePatients = appointments.Select(a => a.PatientID).Distinct().Count();
            var completedCount = appointments.Count(a => a.Status == "Completed");

            ViewBag.Doctor = doc;
            ViewBag.Appointments = appointments;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.UniquePatients = uniquePatients;
            ViewBag.CompletedCount = completedCount;

            return PartialView("_StatsPanel", doc);
        }

        // GET: /Doctors/Create (Partial View for Modal)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new DoctorViewModel { PaymentGatewayIsActive = true };
            var isPlatformOwner = User.IsPlatformOwner();
            var userOrgId = User.GetOrganizationId();

            if (isPlatformOwner)
            {
                var orgs = await _uow.Organizations.GetAllAsync();
                ViewBag.Organizations = new SelectList(orgs.Where(o => o.IsActive), "OrganizationID", "OrganizationName");
                ViewBag.Branches = new SelectList(new List<Branch>(), "BranchID", "BranchName");
                ViewBag.Departments = new SelectList(new List<Department>(), "DepartmentID", "DepartmentName");
            }
            else if (userOrgId.HasValue)
            {
                model.OrganizationID = userOrgId.Value;
                var branches = await _uow.Branches.GetByOrganizationIdAsync(userOrgId.Value);
                var departments = await _uow.Departments.GetByOrganizationIdAsync(userOrgId.Value);
                ViewBag.Branches = new SelectList(branches.Where(b => b.IsActive == true), "BranchID", "BranchName");
                ViewBag.Departments = new SelectList(departments.Where(d => d.IsActive == true), "DepartmentID", "DepartmentName");
            }

            var specs = await _uow.Specializations.GetAllAsync();
            ViewBag.Specializations = specs.Where(s => s.IsActive == true).ToList();

            return PartialView("_Create", model);
        }

        // POST: /Doctors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DoctorViewModel model)
        {
            var isPlatformOwner = User.IsPlatformOwner();
            var userOrgId = User.GetOrganizationId();

            // Override OrganizationID if not platform owner
            if (!isPlatformOwner && userOrgId.HasValue)
            {
                model.OrganizationID = userOrgId.Value;
            }

            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", "Password is required for new doctors.");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Validation failed.", errors });
            }

            // Verify selected branch and department belong to organization
            if (model.BranchID.HasValue)
            {
                var branch = await _uow.Branches.GetByIdAsync(model.BranchID.Value);
                if (branch == null || branch.OrganizationID != model.OrganizationID)
                {
                    return Json(new { success = false, message = "The selected branch is invalid or does not belong to your organization." });
                }
            }
            if (model.DepartmentID.HasValue)
            {
                var dept = await _uow.Departments.GetByIdAsync(model.DepartmentID.Value);
                if (dept == null || dept.OrganizationID != model.OrganizationID)
                {
                    return Json(new { success = false, message = "The selected department is invalid or does not belong to your organization." });
                }
            }

            // Check duplicate email
            var existingUser = await _uow.Users.GetByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return Json(new { success = false, message = "Email address is already in use by another user." });
            }

            // Payment Gateway (Razorpay) - only Organization Admin may configure credentials.
            // Doctors have no access to this controller at all, so they can never view/edit this.
            var canManagePaymentGateway = User.IsInRole("Organization Admin");
            var razorpayKeyId = canManagePaymentGateway ? model.RazorpayKeyId?.Trim() : null;
            var razorpayKeySecret = canManagePaymentGateway ? model.RazorpayKeySecret?.Trim() : null;
            if (canManagePaymentGateway
                && (model.PaymentGatewayIsActive || !string.IsNullOrEmpty(razorpayKeyId) || !string.IsNullOrEmpty(razorpayKeySecret))
                && (string.IsNullOrEmpty(razorpayKeyId) || string.IsNullOrEmpty(razorpayKeySecret)))
            {
                return Json(new { success = false, message = "Enter both Razorpay Key ID and Key Secret to enable online payments for this doctor." });
            }

            // Find Doctor role
            var roles = await _uow.Roles.GetAllAsync();
            var doctorRole = roles.FirstOrDefault(r => r.RoleName.Equals("Doctor", StringComparison.OrdinalIgnoreCase));
            if (doctorRole == null)
            {
                return Json(new { success = false, message = "Doctor role not configured in the system database." });
            }

            await _uow.BeginTransactionAsync();
            try
            {
                string? profileImagePath = null;
                if (model.ProfileImageFile != null)
                {
                    profileImagePath = await CmsImageUploadHelper.UploadAsync(model.ProfileImageFile, _env.WebRootPath, model.OrganizationID, "doctors", 3 * 1024 * 1024);
                }

                // Create user
                var user = new User
                {
                    OrganizationID = model.OrganizationID,
                    RoleID = doctorRole.RoleID,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone,
                    PasswordHash = PasswordHasher.HashPassword(model.Password!),
                    ProfileImage = profileImagePath,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.Now
                };

                var userId = await _uow.Users.AddAsync(user);

                // Create doctor
                var doctor = new Doctor
                {
                    UserID = userId,
                    OrganizationID = model.OrganizationID,
                    BranchID = model.BranchID,
                    DepartmentID = model.DepartmentID,
                    Qualification = model.Qualification,
                    ExperienceYears = model.ExperienceYears,
                    MedicalRegistrationNumber = model.MedicalRegistrationNumber,
                    Biography = model.Biography,
                    ConsultationFee = model.ConsultationFee,
                    VideoConsultationFee = model.VideoConsultationFee,
                    VoiceConsultationFee = model.VoiceConsultationFee,
                    PriorityConsultationFee = model.PriorityConsultationFee,
                    IsPriorityAvailable = model.IsPriorityAvailable,
                    PriorityStartTime = model.PriorityStartTime,
                    PriorityEndTime = model.PriorityEndTime,
                    IsActive = model.IsActive
                };

                var doctorId = await _uow.Doctors.AddAsync(doctor);

                // Assign specializations
                await _uow.Doctors.AddDoctorSpecializationsAsync(doctorId, model.SpecializationIDs);

                if (canManagePaymentGateway && !string.IsNullOrEmpty(razorpayKeyId) && !string.IsNullOrEmpty(razorpayKeySecret))
                {
                    await _uow.DoctorPaymentGateways.AddAsync(new DoctorPaymentGateway
                    {
                        OrganizationID = model.OrganizationID,
                        DoctorID = doctorId,
                        PaymentProvider = "Razorpay",
                        KeyID = razorpayKeyId,
                        KeySecret = razorpayKeySecret,
                        IsActive = model.PaymentGatewayIsActive
                    });
                }

                await _uow.CommitAsync();
                return Json(new { success = true, message = "Doctor created successfully." });
            }
            catch (CmsUploadValidationException ex)
            {
                await _uow.RollbackAsync();
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // GET: /Doctors/Edit/5 (Partial View for Modal)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userOrgId = User.GetOrganizationId();
            var doc = await _uow.Doctors.GetDoctorDetailsByIdAsync(id, User.IsPlatformOwner() ? null : userOrgId);
            if (doc == null)
            {
                return NotFound("Doctor profile not found.");
            }

            var model = new DoctorViewModel
            {
                DoctorID = doc.DoctorID,
                UserID = doc.UserID,
                OrganizationID = doc.OrganizationID,
                BranchID = doc.BranchID,
                DepartmentID = doc.DepartmentID,
                Qualification = doc.Qualification,
                ExperienceYears = doc.ExperienceYears,
                MedicalRegistrationNumber = doc.MedicalRegistrationNumber,
                Biography = doc.Biography,
                ConsultationFee = doc.ConsultationFee,
                VideoConsultationFee = doc.VideoConsultationFee,
                VoiceConsultationFee = doc.VoiceConsultationFee,
                PriorityConsultationFee = doc.PriorityConsultationFee,
                IsPriorityAvailable = doc.IsPriorityAvailable,
                PriorityStartTime = doc.PriorityStartTime,
                PriorityEndTime = doc.PriorityEndTime,
                IsActive = doc.IsActive,
                FirstName = doc.FirstName,
                LastName = doc.LastName,
                Email = doc.Email,
                Phone = doc.Phone,
                ExistingProfileImage = doc.ProfileImage,
                SpecializationIDs = doc.SpecializationIDs
            };

            if (User.IsInRole("Organization Admin"))
            {
                var gateway = await _uow.DoctorPaymentGateways.GetByDoctorIdAsync(doc.DoctorID);
                if (gateway != null)
                {
                    model.HasPaymentGateway = true;
                    model.RazorpayKeyId = gateway.KeyID;
                    model.RazorpayKeySecretMasked = MaskSecret(gateway.KeySecret);
                    model.PaymentGatewayIsActive = gateway.IsActive;
                }
                else
                {
                    model.PaymentGatewayIsActive = true;
                }
            }

            var isPlatformOwner = User.IsPlatformOwner();
            if (isPlatformOwner)
            {
                var orgs = await _uow.Organizations.GetAllAsync();
                ViewBag.Organizations = new SelectList(orgs.Where(o => o.IsActive), "OrganizationID", "OrganizationName", doc.OrganizationID);
                
                var branches = await _uow.Branches.GetByOrganizationIdAsync(doc.OrganizationID);
                var departments = await _uow.Departments.GetByOrganizationIdAsync(doc.OrganizationID);
                ViewBag.Branches = new SelectList(branches.Where(b => b.IsActive == true), "BranchID", "BranchName", doc.BranchID);
                ViewBag.Departments = new SelectList(departments.Where(d => d.IsActive == true), "DepartmentID", "DepartmentName", doc.DepartmentID);
            }
            else if (userOrgId.HasValue)
            {
                var branches = await _uow.Branches.GetByOrganizationIdAsync(userOrgId.Value);
                var departments = await _uow.Departments.GetByOrganizationIdAsync(userOrgId.Value);
                ViewBag.Branches = new SelectList(branches.Where(b => b.IsActive == true), "BranchID", "BranchName", doc.BranchID);
                ViewBag.Departments = new SelectList(departments.Where(d => d.IsActive == true), "DepartmentID", "DepartmentName", doc.DepartmentID);
            }

            var specs = await _uow.Specializations.GetAllAsync();
            ViewBag.Specializations = specs.Where(s => s.IsActive == true).ToList();

            return PartialView("_Edit", model);
        }

        // POST: /Doctors/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DoctorViewModel model)
        {
            var isPlatformOwner = User.IsPlatformOwner();
            var userOrgId = User.GetOrganizationId();

            if (!isPlatformOwner && userOrgId.HasValue)
            {
                model.OrganizationID = userOrgId.Value;
            }

            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Validation failed.", errors });
            }

            // Verify selected branch and department belong to organization
            if (model.BranchID.HasValue)
            {
                var branch = await _uow.Branches.GetByIdAsync(model.BranchID.Value);
                if (branch == null || branch.OrganizationID != model.OrganizationID)
                {
                    return Json(new { success = false, message = "The selected branch is invalid or does not belong to your organization." });
                }
            }
            if (model.DepartmentID.HasValue)
            {
                var dept = await _uow.Departments.GetByIdAsync(model.DepartmentID.Value);
                if (dept == null || dept.OrganizationID != model.OrganizationID)
                {
                    return Json(new { success = false, message = "The selected department is invalid or does not belong to your organization." });
                }
            }

            // Check duplicate email
            var existingUser = await _uow.Users.GetByEmailAsync(model.Email);
            if (existingUser != null && existingUser.UserID != model.UserID)
            {
                return Json(new { success = false, message = "Email address is already in use by another user." });
            }

            var existingDoc = await _uow.Doctors.GetByIdAsync(model.DoctorID);
            if (existingDoc == null || (userOrgId.HasValue && existingDoc.OrganizationID != userOrgId.Value))
            {
                return Json(new { success = false, message = "Doctor profile not found." });
            }

            // Payment Gateway (Razorpay) - only Organization Admin may configure credentials.
            // A blank Key Secret means "keep the existing one" (it's never sent back to the browser);
            // a blank Key ID means "leave the stored config untouched, just apply the Active toggle".
            var canManagePaymentGateway = User.IsInRole("Organization Admin");
            DoctorPaymentGateway? existingGateway = null;
            string? razorpayKeyId = null;
            string? razorpayKeySecretInput = null;
            if (canManagePaymentGateway)
            {
                existingGateway = await _uow.DoctorPaymentGateways.GetByDoctorIdAsync(model.DoctorID);
                razorpayKeyId = model.RazorpayKeyId?.Trim();
                razorpayKeySecretInput = model.RazorpayKeySecret?.Trim();
                var willHaveSecret = !string.IsNullOrEmpty(razorpayKeySecretInput) || !string.IsNullOrEmpty(existingGateway?.KeySecret);

                if ((model.PaymentGatewayIsActive || !string.IsNullOrEmpty(razorpayKeyId) || !string.IsNullOrEmpty(razorpayKeySecretInput))
                    && (string.IsNullOrEmpty(razorpayKeyId) || !willHaveSecret))
                {
                    return Json(new { success = false, message = "Enter both Razorpay Key ID and Key Secret to enable online payments for this doctor." });
                }
            }

            await _uow.BeginTransactionAsync();
            try
            {
                // Update User
                var user = await _uow.Users.GetByIdAsync(model.UserID);
                if (user != null)
                {
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.Email = model.Email;
                    user.Phone = model.Phone;
                    user.IsActive = model.IsActive;

                    if (model.ProfileImageFile != null)
                    {
                        var newProfileImage = await CmsImageUploadHelper.UploadAsync(model.ProfileImageFile, _env.WebRootPath, model.OrganizationID, "doctors", 3 * 1024 * 1024);
                        CmsImageUploadHelper.DeleteIfExists(_env.WebRootPath, user.ProfileImage);
                        user.ProfileImage = newProfileImage;
                    }

                    await _uow.Users.UpdateAsync(user);
                }

                // Update Doctor
                existingDoc.BranchID = model.BranchID;
                existingDoc.DepartmentID = model.DepartmentID;
                existingDoc.Qualification = model.Qualification;
                existingDoc.ExperienceYears = model.ExperienceYears;
                existingDoc.MedicalRegistrationNumber = model.MedicalRegistrationNumber;
                existingDoc.Biography = model.Biography;
                existingDoc.ConsultationFee = model.ConsultationFee;
                existingDoc.VideoConsultationFee = model.VideoConsultationFee;
                existingDoc.VoiceConsultationFee = model.VoiceConsultationFee;
                existingDoc.PriorityConsultationFee = model.PriorityConsultationFee;
                existingDoc.IsPriorityAvailable = model.IsPriorityAvailable;
                existingDoc.PriorityStartTime = model.PriorityStartTime;
                existingDoc.PriorityEndTime = model.PriorityEndTime;
                existingDoc.IsActive = model.IsActive;

                await _uow.Doctors.UpdateAsync(existingDoc);

                // Update specializations
                await _uow.Doctors.ClearDoctorSpecializationsAsync(model.DoctorID);
                await _uow.Doctors.AddDoctorSpecializationsAsync(model.DoctorID, model.SpecializationIDs);

                if (canManagePaymentGateway)
                {
                    if (!string.IsNullOrEmpty(razorpayKeyId))
                    {
                        var keySecretToStore = !string.IsNullOrEmpty(razorpayKeySecretInput)
                            ? razorpayKeySecretInput
                            : existingGateway?.KeySecret ?? string.Empty;

                        await _uow.DoctorPaymentGateways.UpsertAsync(new DoctorPaymentGateway
                        {
                            OrganizationID = model.OrganizationID,
                            DoctorID = model.DoctorID,
                            PaymentProvider = "Razorpay",
                            KeyID = razorpayKeyId,
                            KeySecret = keySecretToStore,
                            IsActive = model.PaymentGatewayIsActive
                        });
                    }
                    else if (existingGateway != null)
                    {
                        existingGateway.IsActive = model.PaymentGatewayIsActive;
                        await _uow.DoctorPaymentGateways.UpdateAsync(existingGateway);
                    }
                }

                await _uow.CommitAsync();
                return Json(new { success = true, message = "Doctor profile updated successfully." });
            }
            catch (CmsUploadValidationException ex)
            {
                await _uow.RollbackAsync();
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // POST: /Doctors/ToggleStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            var userOrgId = User.GetOrganizationId();
            var doc = await _uow.Doctors.GetByIdAsync(id);
            if (doc == null || (userOrgId.HasValue && doc.OrganizationID != userOrgId.Value))
            {
                return Json(new { success = false, message = "Doctor not found or unauthorized access." });
            }

            await _uow.BeginTransactionAsync();
            try
            {
                // Toggle User and Doctor status
                await _uow.Doctors.UpdateStatusAsync(id, isActive);
                await _uow.Users.UpdateStatusAsync(doc.UserID, isActive);

                await _uow.CommitAsync();
                return Json(new { success = true, message = $"Doctor status {(isActive ? "activated" : "deactivated")} successfully." });
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return Json(new { success = false, message = $"Failed to toggle status: {ex.Message}" });
            }
        }

        // GET: /Doctors/GetBranchesAndDepartments
        [HttpGet]
        public async Task<IActionResult> GetBranchesAndDepartments(int orgId)
        {
            // Security constraint: Organization admins cannot view another organization's metadata
            var userOrgId = User.GetOrganizationId();
            if (userOrgId.HasValue && userOrgId.Value != orgId)
            {
                return Forbid();
            }

            var branches = await _uow.Branches.GetByOrganizationIdAsync(orgId);
            var departments = await _uow.Departments.GetByOrganizationIdAsync(orgId);

            return Json(new
            {
                branches = branches.Where(b => b.IsActive == true).Select(b => new { id = b.BranchID, name = b.BranchName }),
                departments = departments.Where(d => d.IsActive == true).Select(d => new { id = d.DepartmentID, name = d.DepartmentName })
            });
        }

        // POST: /Doctors/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var isPlatformOwner = User.IsPlatformOwner();
                var userOrgId = User.GetOrganizationId();

                var doctor = await _uow.Doctors.GetByIdAsync(id);
                if (doctor == null)
                {
                    return Json(new { success = false, message = "Doctor profile not found." });
                }

                // If not Platform Owner, check if Doctor belongs to the same Organization
                if (!isPlatformOwner && doctor.OrganizationID != userOrgId)
                {
                    return Json(new { success = false, message = "Unauthorized access to delete this doctor." });
                }

                // Delete image if exists
                var user = await _uow.Users.GetByIdAsync(doctor.UserID);
                if (user != null && !string.IsNullOrEmpty(user.ProfileImage))
                {
                    CmsImageUploadHelper.DeleteIfExists(_env.WebRootPath, user.ProfileImage);
                }

                // Delete Doctor and User
                await _uow.Doctors.DeleteAsync(id);
                await _uow.Users.DeleteAsync(doctor.UserID);

                return Json(new { success = true, message = "Doctor profile and user account deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // Never returns the real secret - only the last 4 characters, for the admin to recognize
        // which credential is on file without re-exposing the full value to the browser.
        private static string MaskSecret(string secret)
        {
            if (string.IsNullOrEmpty(secret)) return string.Empty;
            var visible = secret.Length > 4 ? secret[^4..] : secret;
            return new string('•', 8) + visible;
        }
    }
}
