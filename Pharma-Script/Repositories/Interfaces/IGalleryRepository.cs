using Pharma_Script.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface IGalleryRepository : IRepository<GalleryImage>
    {
        Task<IEnumerable<GalleryImage>> GetByOrganizationIdAsync(int organizationId);
        Task<IEnumerable<GalleryImage>> GetActiveByOrganizationIdAsync(int organizationId);
        Task<GalleryImage?> GetByIdForOrganizationAsync(int id, int organizationId);
        Task<bool> SetActiveAsync(int id, int organizationId, bool isActive);
        Task<bool> UpdateDisplayOrderAsync(int id, int organizationId, int displayOrder);
        Task<bool> DeleteForOrganizationAsync(int id, int organizationId);
    }
}
