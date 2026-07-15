using System.Threading.Tasks;
using Pharma_Script.Models;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IConsultationSessionRepository
    {
        Task<ConsultationSession?> GetByAppointmentIdAsync(int appointmentId, int organizationId);
        Task<ConsultationSession?> GetByIdAsync(int sessionId, int organizationId);
        Task<int> CreateAsync(ConsultationSession session);
        Task<bool> UpdateAsync(ConsultationSession session);
    }
}
