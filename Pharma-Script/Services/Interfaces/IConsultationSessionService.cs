using System.Threading.Tasks;
using Pharma_Script.Models;

namespace Pharma_Script.Services.Interfaces
{
    public interface IConsultationSessionService
    {
        Task<ConsultationSession> GetOrCreateSessionAsync(int appointmentId, int organizationId);
        Task<bool> UpdateVideoLinkAsync(int appointmentId, string provider, string url, int organizationId);
        Task<bool> UpdateSessionStatusAsync(int appointmentId, string status, int organizationId);
    }
}
