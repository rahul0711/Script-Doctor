using Pharma_Script.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IFollowUpRepository : IRepository<FollowUp>
    {
        Task<FollowUp?> GetByAppointmentIdAsync(int appointmentId, int? orgId);
        Task<IEnumerable<FollowUp>> GetUpcomingFollowUpsAsync(int? orgId, int? doctorId, int? patientId);
        Task<bool> UpdateStatusAsync(int followUpId, string status, int? orgId);
    }
}
