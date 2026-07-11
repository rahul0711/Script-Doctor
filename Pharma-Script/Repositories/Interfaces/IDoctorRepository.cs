using Pharma_Script.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IDoctorRepository : IRepository<Doctor>
    {
        Task<IEnumerable<Doctor>> SearchAndPaginateAsync(int? orgId, int? branchId, int? departmentId, int? specializationId, bool? isActive, string searchTerm, int page, int pageSize);
        Task<int> GetSearchCountAsync(int? orgId, int? branchId, int? departmentId, int? specializationId, bool? isActive, string searchTerm);
        Task<Doctor?> GetDoctorDetailsByIdAsync(int id, int? orgId);
        Task<bool> UpdateStatusAsync(int id, bool isActive);
        Task<IEnumerable<int>> GetDoctorSpecializationIDsAsync(int doctorId);
        Task<bool> ClearDoctorSpecializationsAsync(int doctorId);
        Task<bool> AddDoctorSpecializationsAsync(int doctorId, List<int> specializationIds);
        Task<Doctor?> GetByUserIdAsync(int userId);
    }
}
