using Pharma_Script.Models;
using System;
using System.Threading.Tasks;

namespace Pharma_Script.Services.Interfaces
{
    public class PatientProvisioningRequest
    {
        public int OrganizationID { get; set; }
        public int? BranchID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string? BloodGroup { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? Pincode { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // Thrown for expected, user-facing provisioning failures (e.g. duplicate email,
    // missing Patient role) so callers can show a friendly message instead of a raw error.
    public class PatientProvisioningException : Exception
    {
        public PatientProvisioningException(string message) : base(message) { }
    }

    public interface IPatientProvisioningService
    {
        Task<Patient> CreatePatientAsync(PatientProvisioningRequest request);
    }
}
