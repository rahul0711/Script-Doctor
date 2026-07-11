using Pharma_Script.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IDoctorAvailabilityRepository : IRepository<DoctorAvailability>
    {
        Task<IEnumerable<DoctorAvailability>> GetAvailabilityByDoctorIdAsync(int doctorId);
        Task<bool> CheckOverlapAsync(int doctorId, string dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeId);
    }
}
