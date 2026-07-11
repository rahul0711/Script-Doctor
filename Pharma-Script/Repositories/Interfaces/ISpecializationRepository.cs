using Pharma_Script.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface ISpecializationRepository : IRepository<Specialization>
    {
        Task<IEnumerable<Specialization>> SearchAndPaginateAsync(string searchTerm, int page, int pageSize);
        Task<int> GetSearchCountAsync(string searchTerm);
    }
}
