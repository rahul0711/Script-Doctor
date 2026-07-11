using Pharma_Script.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IMedicalDocumentRepository : IRepository<MedicalDocument>
    {
        Task<IEnumerable<MedicalDocument>> GetByPatientIdAsync(int patientId, int? orgId);
    }
}
