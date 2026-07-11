using Pharma_Script.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IDoctorLeaveRepository : IRepository<DoctorLeave>
    {
        Task<IEnumerable<DoctorLeave>> GetUpcomingLeavesByDoctorIdAsync(int doctorId);
        Task<IEnumerable<DoctorLeave>> GetPastLeavesByDoctorIdAsync(int doctorId);
        Task<bool> CheckLeaveOverlapAsync(int doctorId, DateTime startDate, DateTime endDate, int? excludeId);
    }
}
