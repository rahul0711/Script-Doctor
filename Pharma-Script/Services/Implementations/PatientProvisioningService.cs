using Pharma_Script.Helpers;
using Pharma_Script.Models;
using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pharma_Script.Services.Implementations
{
    // Single source of truth for creating a Patient + its backing User account.
    // Used by both the internal staff-facing PatientsController and the public
    // self-registration flow, so the transaction is never duplicated.
    public class PatientProvisioningService : IPatientProvisioningService
    {
        private readonly IUnitOfWork _uow;

        public PatientProvisioningService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Patient> CreatePatientAsync(PatientProvisioningRequest request)
        {
            var existingUser = await _uow.Users.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new PatientProvisioningException("An account with this email already exists.");
            }

            var roles = await _uow.Roles.GetAllAsync();
            var patientRole = roles.FirstOrDefault(r => r.RoleName.Equals("Patient", StringComparison.OrdinalIgnoreCase));
            if (patientRole == null)
            {
                throw new PatientProvisioningException("Patient role is not configured for this system.");
            }

            await _uow.BeginTransactionAsync();
            try
            {
                var user = new User
                {
                    OrganizationID = request.OrganizationID,
                    RoleID = patientRole.RoleID,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Phone = request.Phone,
                    PasswordHash = PasswordHasher.HashPassword(request.Password),
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.Now
                };

                var userId = await _uow.Users.AddAsync(user);

                var patient = new Patient
                {
                    UserID = userId,
                    OrganizationID = request.OrganizationID,
                    BranchID = request.BranchID,
                    DateOfBirth = request.DateOfBirth,
                    Gender = request.Gender,
                    BloodGroup = request.BloodGroup,
                    Height = request.Height,
                    Weight = request.Weight,
                    EmergencyContactName = request.EmergencyContactName,
                    EmergencyContactNumber = request.EmergencyContactNumber,
                    Address = request.Address,
                    City = request.City,
                    State = request.State,
                    Country = request.Country,
                    Pincode = request.Pincode,
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.Now
                };

                var patientId = await _uow.Patients.AddAsync(patient);
                patient.PatientID = patientId;

                var history = new PatientMedicalHistory { PatientID = patientId };
                await _uow.PatientMedicalHistories.AddAsync(history);

                if (request.Height.HasValue && request.Weight.HasValue)
                {
                    var heightMeters = request.Height.Value;
                    if (heightMeters > 3) heightMeters /= 100;

                    var vitals = new PatientVitals
                    {
                        PatientID = patientId,
                        Height = request.Height,
                        Weight = request.Weight,
                        BMI = request.Weight.Value / (heightMeters * heightMeters)
                    };
                    await _uow.PatientVitals.AddAsync(vitals);
                }

                await _uow.CommitAsync();
                return patient;
            }
            catch
            {
                await _uow.RollbackAsync();
                throw;
            }
        }
    }
}
