using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pharma_Script.Helpers;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.Services.Interfaces;
using Pharma_Script.ViewModels.Patient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pharma_Script.Controllers
{
    [Authorize(Roles = "Platform Owner,Organization Admin,Doctor,Receptionist")]
    public class PatientsController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IPatientProvisioningService _patientProvisioningService;

        public PatientsController(IUnitOfWork uow, IPatientProvisioningService patientProvisioningService)
        {
            _uow = uow;
            _patientProvisioningService = patientProvisioningService;
        }

        // GET: /Patients
        public async Task<IActionResult> Index(int? orgIdFilter, int? branchId, string searchTerm, int page = 1, int pageSize = 10)
        {
            var isPlatformOwner = User.IsPlatformOwner();
            var userOrgId = User.GetOrganizationId();
            var activeOrgId = isPlatformOwner ? orgIdFilter : userOrgId;

            IEnumerable<Branch> branches;
            if (isPlatformOwner)
            {
                var organizations = await _uow.Organizations.GetAllAsync();
                ViewBag.Organizations = new SelectList(organizations.Where(o => o.IsActive), "OrganizationID", "OrganizationName", orgIdFilter);

                branches = activeOrgId.HasValue ? await _uow.Branches.GetByOrganizationIdAsync(activeOrgId.Value) : await _uow.Branches.GetAllAsync();
            }
            else if (userOrgId.HasValue)
            {
                branches = await _uow.Branches.GetByOrganizationIdAsync(userOrgId.Value);
            }
            else
            {
                branches = new List<Branch>();
            }

            ViewBag.Branches = new SelectList(branches.Where(b => b.IsActive == true), "BranchID", "BranchName", branchId);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedOrg = orgIdFilter;
            ViewBag.SelectedBranch = branchId;

            var patients = await _uow.Patients.SearchAndPaginateAsync(activeOrgId, branchId, searchTerm, page, pageSize);
            var totalItems = await _uow.Patients.GetSearchCountAsync(activeOrgId, branchId, searchTerm);

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(patients);
        }

        // GET: /Patients/Details/5 (Full EMR Dashboard Page)
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userOrgId = User.GetOrganizationId();
            var patient = await _uow.Patients.GetPatientDetailsByIdAsync(id, User.IsPlatformOwner() ? null : userOrgId);
            if (patient == null)
            {
                return NotFound("Patient profile not found.");
            }

            var medicalHistory = await _uow.PatientMedicalHistories.GetByPatientIdAsync(id) ?? new PatientMedicalHistory { PatientID = id };
            var latestVitals = await _uow.PatientVitals.GetLatestByPatientIdAsync(id);
            var vitalsHistory = await _uow.PatientVitals.GetHistoryByPatientIdAsync(id);
            var documents = await _uow.MedicalDocuments.GetByPatientIdAsync(id, User.IsPlatformOwner() ? null : userOrgId);

            ViewBag.MedicalHistory = medicalHistory;
            ViewBag.LatestVitals = latestVitals;
            ViewBag.VitalsHistory = vitalsHistory;
            ViewBag.Documents = documents;

            return View(patient);
        }

        // GET: /Patients/DoctorHistory/5 (Partial view – doctors visited by this patient)
        [HttpGet]
        public async Task<IActionResult> DoctorHistory(int id)
        {
            var userOrgId = User.GetOrganizationId();
            var isPlatformOwner = User.IsPlatformOwner();

            var patient = await _uow.Patients.GetPatientDetailsByIdAsync(id, isPlatformOwner ? null : userOrgId);
            if (patient == null)
                return NotFound("Patient profile not found.");

            var appointments = (await _uow.Appointments.GetByPatientIdAsync(id, isPlatformOwner ? null : userOrgId)).ToList();

            // Group by doctor and compute per-doctor stats
            var doctorGroups = appointments
                .GroupBy(a => a.DoctorID)
                .Select(g => new
                {
                    DoctorID = g.Key,
                    DoctorName = g.First().DoctorName,
                    AppointmentCount = g.Count(),
                    CompletedCount = g.Count(a => a.Status == "Completed"),
                    TotalFee = g.Sum(a => a.ConsultationFee),
                    LastVisit = g.Max(a => a.AppointmentDate)
                })
                .OrderByDescending(x => x.LastVisit)
                .ToList();

            // Payments for this patient in the org
            IEnumerable<Pharma_Script.Models.Payment> payments = new List<Pharma_Script.Models.Payment>();
            if (userOrgId.HasValue && !isPlatformOwner)
            {
                payments = await _uow.Payments.GetByPatientAndOrgAsync(id, userOrgId.Value);
            }
            else if (isPlatformOwner && appointments.Any())
            {
                // For platform owner, aggregate across all orgs the patient belongs to
                var orgId = appointments.First().OrganizationID;
                payments = await _uow.Payments.GetByPatientAndOrgAsync(id, orgId);
            }

            ViewBag.Patient = patient;
            ViewBag.Appointments = appointments;
            ViewBag.DoctorGroups = doctorGroups;
            ViewBag.Payments = payments.ToList();
            ViewBag.TotalPaid = payments.Where(p => p.PaymentStatus == "Paid").Sum(p => p.Amount);

            return PartialView("_DoctorHistory", patient);
        }

        // GET: /Patients/Create (Partial View Modal)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new PatientViewModel();
            var isPlatformOwner = User.IsPlatformOwner();
            var userOrgId = User.GetOrganizationId();

            if (isPlatformOwner)
            {
                var orgs = await _uow.Organizations.GetAllAsync();
                ViewBag.Organizations = new SelectList(orgs.Where(o => o.IsActive), "OrganizationID", "OrganizationName");
                ViewBag.Branches = new SelectList(new List<Branch>(), "BranchID", "BranchName");
            }
            else if (userOrgId.HasValue)
            {
                model.OrganizationID = userOrgId.Value;
                var branches = await _uow.Branches.GetByOrganizationIdAsync(userOrgId.Value);
                ViewBag.Branches = new SelectList(branches.Where(b => b.IsActive == true), "BranchID", "BranchName");
            }

            return PartialView("_Create", model);
        }

        // POST: /Patients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PatientViewModel model)
        {
            var isPlatformOwner = User.IsPlatformOwner();
            var userOrgId = User.GetOrganizationId();

            if (!isPlatformOwner && userOrgId.HasValue)
            {
                model.OrganizationID = userOrgId.Value;
            }

            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", "Password is required for new patients.");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Validation failed.", errors });
            }

            if (model.BranchID.HasValue)
            {
                var branch = await _uow.Branches.GetByIdAsync(model.BranchID.Value);
                if (branch == null || branch.OrganizationID != model.OrganizationID)
                {
                    return Json(new { success = false, message = "Invalid branch selected." });
                }
            }

            try
            {
                await _patientProvisioningService.CreatePatientAsync(new PatientProvisioningRequest
                {
                    OrganizationID = model.OrganizationID,
                    BranchID = model.BranchID,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Password = model.Password!,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    BloodGroup = model.BloodGroup,
                    Height = model.Height,
                    Weight = model.Weight,
                    EmergencyContactName = model.EmergencyContactName,
                    EmergencyContactNumber = model.EmergencyContactNumber,
                    Address = model.Address,
                    City = model.City,
                    State = model.State,
                    Country = model.Country,
                    Pincode = model.Pincode,
                    IsActive = model.IsActive
                });

                return Json(new { success = true, message = "Patient registered successfully." });
            }
            catch (PatientProvisioningException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Registration failed: {ex.Message}" });
            }
        }

        // GET: /Patients/Edit/5 (Partial View Modal)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userOrgId = User.GetOrganizationId();
            var patient = await _uow.Patients.GetByIdAsync(id);
            if (patient == null || (!User.IsPlatformOwner() && userOrgId.HasValue && patient.OrganizationID != userOrgId.Value))
            {
                return NotFound("Patient profile not found.");
            }

            var user = await _uow.Users.GetByIdAsync(patient.UserID);
            if (user == null)
            {
                return NotFound("User profile not found.");
            }

            var model = new PatientViewModel
            {
                PatientID = patient.PatientID,
                UserID = patient.UserID,
                OrganizationID = patient.OrganizationID,
                BranchID = patient.BranchID,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                BloodGroup = patient.BloodGroup,
                Height = patient.Height,
                Weight = patient.Weight,
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactNumber = patient.EmergencyContactNumber,
                Address = patient.Address,
                City = patient.City,
                State = patient.State,
                Country = patient.Country,
                Pincode = patient.Pincode,
                IsActive = patient.IsActive,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone
            };

            var isPlatformOwner = User.IsPlatformOwner();
            if (isPlatformOwner)
            {
                var orgs = await _uow.Organizations.GetAllAsync();
                ViewBag.Organizations = new SelectList(orgs.Where(o => o.IsActive), "OrganizationID", "OrganizationName", patient.OrganizationID);
                var branches = await _uow.Branches.GetByOrganizationIdAsync(patient.OrganizationID);
                ViewBag.Branches = new SelectList(branches.Where(b => b.IsActive == true), "BranchID", "BranchName", patient.BranchID);
            }
            else if (userOrgId.HasValue)
            {
                var branches = await _uow.Branches.GetByOrganizationIdAsync(userOrgId.Value);
                ViewBag.Branches = new SelectList(branches.Where(b => b.IsActive == true), "BranchID", "BranchName", patient.BranchID);
            }

            return PartialView("_Edit", model);
        }

        // POST: /Patients/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PatientViewModel model)
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

            var patient = await _uow.Patients.GetByIdAsync(model.PatientID);
            if (patient == null || (userOrgId.HasValue && patient.OrganizationID != userOrgId.Value))
            {
                return Json(new { success = false, message = "Patient profile not found." });
            }

            var existingUser = await _uow.Users.GetByEmailAsync(model.Email);
            if (existingUser != null && existingUser.UserID != model.UserID)
            {
                return Json(new { success = false, message = "Email already in use." });
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
                    await _uow.Users.UpdateAsync(user);
                }

                // Update Patient
                patient.BranchID = model.BranchID;
                patient.DateOfBirth = model.DateOfBirth;
                patient.Gender = model.Gender;
                patient.BloodGroup = model.BloodGroup;
                patient.Height = model.Height;
                patient.Weight = model.Weight;
                patient.EmergencyContactName = model.EmergencyContactName;
                patient.EmergencyContactNumber = model.EmergencyContactNumber;
                patient.Address = model.Address;
                patient.City = model.City;
                patient.State = model.State;
                patient.Country = model.Country;
                patient.Pincode = model.Pincode;
                patient.IsActive = model.IsActive;

                await _uow.Patients.UpdateAsync(patient);

                await _uow.CommitAsync();
                return Json(new { success = true, message = "Patient profile updated successfully." });
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return Json(new { success = false, message = $"Update failed: {ex.Message}" });
            }
        }

        // POST: /Patients/ToggleStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id, bool isActive)
        {
            var userOrgId = User.GetOrganizationId();
            var patient = await _uow.Patients.GetByIdAsync(id);
            if (patient == null || (userOrgId.HasValue && patient.OrganizationID != userOrgId.Value))
            {
                return Json(new { success = false, message = "Patient profile not found." });
            }

            await _uow.BeginTransactionAsync();
            try
            {
                await _uow.Patients.UpdateStatusAsync(id, isActive);
                await _uow.Users.UpdateStatusAsync(patient.UserID, isActive);

                await _uow.CommitAsync();
                return Json(new { success = true, message = $"Patient access {(isActive ? "activated" : "deactivated")} successfully." });
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return Json(new { success = false, message = $"Failed to change status: {ex.Message}" });
            }
        }

        // POST: /Patients/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userOrgId = User.GetOrganizationId();
            var patient = await _uow.Patients.GetByIdAsync(id);
            if (patient == null || (userOrgId.HasValue && patient.OrganizationID != userOrgId.Value))
            {
                return Json(new { success = false, message = "Patient profile not found." });
            }

            await _uow.BeginTransactionAsync();
            try
            {
                await _uow.Patients.DeleteAsync(id);
                await _uow.Users.DeleteAsync(patient.UserID);

                await _uow.CommitAsync();
                return Json(new { success = true, message = "Patient record deleted successfully." });
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return Json(new { success = false, message = $"Deletion failed: {ex.Message}" });
            }
        }

        // POST: /Patients/SaveMedicalHistory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveMedicalHistory(PatientMedicalHistoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Validation errors on history form." });
            }

            var userOrgId = User.GetOrganizationId();
            var patient = await _uow.Patients.GetByIdAsync(model.PatientID);
            if (patient == null || (!User.IsPlatformOwner() && userOrgId.HasValue && patient.OrganizationID != userOrgId.Value))
            {
                return Json(new { success = false, message = "Unauthorized access." });
            }

            try
            {
                var history = await _uow.PatientMedicalHistories.GetByIdAsync(model.MedicalHistoryID);
                bool result;
                if (history == null)
                {
                    history = new PatientMedicalHistory
                    {
                        PatientID = model.PatientID,
                        Diabetes = model.Diabetes,
                        BloodPressure = model.BloodPressure,
                        HeartDisease = model.HeartDisease,
                        Asthma = model.Asthma,
                        Thyroid = model.Thyroid,
                        Allergies = model.Allergies,
                        CurrentMedications = model.CurrentMedications,
                        PastSurgeries = model.PastSurgeries,
                        FamilyMedicalHistory = model.FamilyMedicalHistory,
                        OtherMedicalConditions = model.OtherMedicalConditions
                    };
                    result = await _uow.PatientMedicalHistories.AddAsync(history) > 0;
                }
                else
                {
                    history.Diabetes = model.Diabetes;
                    history.BloodPressure = model.BloodPressure;
                    history.HeartDisease = model.HeartDisease;
                    history.Asthma = model.Asthma;
                    history.Thyroid = model.Thyroid;
                    history.Allergies = model.Allergies;
                    history.CurrentMedications = model.CurrentMedications;
                    history.PastSurgeries = model.PastSurgeries;
                    history.FamilyMedicalHistory = model.FamilyMedicalHistory;
                    history.OtherMedicalConditions = model.OtherMedicalConditions;
                    result = await _uow.PatientMedicalHistories.UpdateAsync(history);
                }

                if (result)
                {
                    return Json(new { success = true, message = "Medical history updated successfully." });
                }
                return Json(new { success = false, message = "No changes were recorded." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        // POST: /Patients/AddVitals
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVitals(PatientVitalsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Validation errors on vitals record.", errors });
            }

            var userOrgId = User.GetOrganizationId();
            var patient = await _uow.Patients.GetByIdAsync(model.PatientID);
            if (patient == null || (!User.IsPlatformOwner() && userOrgId.HasValue && patient.OrganizationID != userOrgId.Value))
            {
                return Json(new { success = false, message = "Unauthorized access." });
            }

            // Server-side BMI computing
            if (model.Height.HasValue && model.Weight.HasValue && model.Height.Value > 0)
            {
                decimal heightM = model.Height.Value;
                if (heightM > 3) heightM /= 100; // convert cm to meters
                model.BMI = model.Weight.Value / (heightM * heightM);
            }

            try
            {
                var vitals = new PatientVitals
                {
                    PatientID = model.PatientID,
                    Height = model.Height,
                    Weight = model.Weight,
                    BloodPressure = model.BloodPressure,
                    HeartRate = model.HeartRate,
                    Temperature = model.Temperature,
                    OxygenLevel = model.OxygenLevel,
                    BloodSugar = model.BloodSugar,
                    BMI = model.BMI,
                    RecordedAt = DateTime.Now
                };

                await _uow.PatientVitals.AddAsync(vitals);
                return Json(new { success = true, message = "Vitals log captured successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Failed to log vitals: {ex.Message}" });
            }
        }

        // POST: /Patients/DeleteVitals/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVitals(int id)
        {
            // Only authorized roles (Platform Owner, Org Admin, Doctor) can delete invalid vitals
            if (!User.IsInRole("Platform Owner") && !User.IsInRole("Organization Admin") && !User.IsInRole("Doctor"))
            {
                return Json(new { success = false, message = "Unauthorized role access to delete clinical records." });
            }

            var vital = await _uow.PatientVitals.GetByIdAsync(id);
            if (vital == null)
            {
                return Json(new { success = false, message = "Vital record not found." });
            }

            var userOrgId = User.GetOrganizationId();
            var patient = await _uow.Patients.GetByIdAsync(vital.PatientID);
            if (patient == null || (!User.IsPlatformOwner() && userOrgId.HasValue && patient.OrganizationID != userOrgId.Value))
            {
                return Json(new { success = false, message = "Unauthorized tenant access." });
            }

            try
            {
                await _uow.PatientVitals.DeleteAsync(id);
                return Json(new { success = true, message = "Vital record removed successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error deleting record: {ex.Message}" });
            }
        }

        // POST: /Patients/UploadDocument
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(MedicalDocumentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Validation failed.", errors });
            }

            var userOrgId = User.GetOrganizationId();
            var patient = await _uow.Patients.GetByIdAsync(model.PatientID);
            if (patient == null || (!User.IsPlatformOwner() && userOrgId.HasValue && patient.OrganizationID != userOrgId.Value))
            {
                return Json(new { success = false, message = "Unauthorized access." });
            }

            if (model.File == null || model.File.Length == 0)
            {
                return Json(new { success = false, message = "Please select a valid non-empty document file to upload." });
            }

            // Server-side extension validation
            var extension = Path.GetExtension(model.File.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            if (!allowedExtensions.Contains(extension))
            {
                return Json(new { success = false, message = "Document format not supported. Only PDF, JPG, JPEG, and PNG are allowed." });
            }

            // Check size (Max 10 MB)
            if (model.File.Length > 10 * 1024 * 1024)
            {
                return Json(new { success = false, message = "File size exceeds the 10 MB limit." });
            }

            try
            {
                // Set path to private folder inside workspace root (outside wwwroot)
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "MedicalDocuments");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                // Safe unique stored filename
                var safeFileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(uploadDir, safeFileName);

                // Write file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(stream);
                }

                var doc = new MedicalDocument
                {
                    PatientID = model.PatientID,
                    OrganizationID = patient.OrganizationID,
                    UploadedByUserID = User.GetUserId(),
                    DocumentTitle = model.DocumentTitle,
                    DocumentType = model.DocumentType,
                    FileName = model.File.FileName,
                    FilePath = safeFileName, // Save only the relative safe filename to prevent path traversal
                    FileSize = model.File.Length,
                    UploadDate = DateTime.Now
                };

                await _uow.MedicalDocuments.AddAsync(doc);
                return Json(new { success = true, message = "Medical document uploaded successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An upload error occurred: {ex.Message}" });
            }
        }

        // GET: /Patients/DownloadDocument/5
        [HttpGet]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            var doc = await _uow.MedicalDocuments.GetByIdAsync(id);
            if (doc == null)
            {
                return NotFound("Document record not found.");
            }

            var userOrgId = User.GetOrganizationId();
            var patient = await _uow.Patients.GetByIdAsync(doc.PatientID);
            if (patient == null || (!User.IsPlatformOwner() && userOrgId.HasValue && patient.OrganizationID != userOrgId.Value))
            {
                return Forbid();
            }

            // Safe path lookup preventing traversal
            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "MedicalDocuments");
            var fullPath = Path.GetFullPath(Path.Combine(uploadDir, doc.FilePath));

            // Prevent path traversal
            if (!fullPath.StartsWith(uploadDir, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Invalid file path lookup.");
            }

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound("File not found on disk storage.");
            }

            // Determine MIME type
            var ext = Path.GetExtension(doc.FileName).ToLowerInvariant();
            var contentType = ext switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };

            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(fileBytes, contentType, doc.FileName);
        }

        // POST: /Patients/DeleteDocument/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            // Only authorized roles (Platform Owner, Org Admin, Doctor) can delete medical documents
            if (!User.IsInRole("Platform Owner") && !User.IsInRole("Organization Admin") && !User.IsInRole("Doctor"))
            {
                return Json(new { success = false, message = "Unauthorized access." });
            }

            var doc = await _uow.MedicalDocuments.GetByIdAsync(id);
            if (doc == null)
            {
                return Json(new { success = false, message = "Document record not found." });
            }

            var userOrgId = User.GetOrganizationId();
            var patient = await _uow.Patients.GetByIdAsync(doc.PatientID);
            if (patient == null || (!User.IsPlatformOwner() && userOrgId.HasValue && patient.OrganizationID != userOrgId.Value))
            {
                return Json(new { success = false, message = "Unauthorized access." });
            }

            try
            {
                // Delete from disk
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "MedicalDocuments");
                var fullPath = Path.GetFullPath(Path.Combine(uploadDir, doc.FilePath));
                if (fullPath.StartsWith(uploadDir, StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                // Delete from DB
                await _uow.MedicalDocuments.DeleteAsync(id);
                return Json(new { success = true, message = "Document deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Deletion failed: {ex.Message}" });
            }
        }
    }
}
