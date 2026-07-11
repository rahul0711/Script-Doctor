using Pharma_Script.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IPatientVitalsRepository : IRepository<PatientVitals>
    {
        Task<PatientVitals?> GetLatestByPatientIdAsync(int patientId);
        Task<IEnumerable<PatientVitals>> GetHistoryByPatientIdAsync(int patientId);
    }
}
