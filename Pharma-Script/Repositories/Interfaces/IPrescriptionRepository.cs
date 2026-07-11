using Pharma_Script.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IPrescriptionRepository : IRepository<Prescription>
    {
        Task<Prescription?> GetByAppointmentIdAsync(int appointmentId, int? orgId);
        Task<Prescription?> GetByPrescriptionNumberAsync(string prescriptionNumber, int? orgId);
        Task<IEnumerable<Prescription>> GetHistoryByPatientIdAsync(int patientId, int? orgId);
        Task<string> GeneratePrescriptionNumberAsync();
    }
}
