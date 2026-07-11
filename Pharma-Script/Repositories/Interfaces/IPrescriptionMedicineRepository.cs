using Pharma_Script.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IPrescriptionMedicineRepository : IRepository<PrescriptionMedicine>
    {
        Task<IEnumerable<PrescriptionMedicine>> GetByPrescriptionIdAsync(int prescriptionId);
        Task<bool> DeleteByPrescriptionIdAsync(int prescriptionId);
    }
}
