using Pharma_Script.Models;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IPatientMedicalHistoryRepository : IRepository<PatientMedicalHistory>
    {
        Task<PatientMedicalHistory?> GetByPatientIdAsync(int patientId);
    }
}
