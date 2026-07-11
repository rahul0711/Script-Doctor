using Pharma_Script.Models;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IDoctorNoteRepository : IRepository<DoctorNote>
    {
        Task<DoctorNote?> GetByAppointmentIdAsync(int appointmentId, int? orgId);
    }
}
