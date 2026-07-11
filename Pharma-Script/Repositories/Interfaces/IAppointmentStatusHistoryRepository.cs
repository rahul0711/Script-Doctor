using Pharma_Script.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IAppointmentStatusHistoryRepository : IRepository<AppointmentStatusHistory>
    {
        Task<IEnumerable<AppointmentStatusHistory>> GetByAppointmentIdAsync(int appointmentId);
    }
}
