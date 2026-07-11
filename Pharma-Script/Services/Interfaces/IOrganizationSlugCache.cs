using Pharma_Script.Repositories.Interfaces;
using System.Threading.Tasks;

namespace Pharma_Script.Services.Interfaces
{
    public interface IOrganizationSlugCache
    {
        Task RefreshAsync(IUnitOfWork uow);
        bool IsActiveSlug(string slug);
        void SetActive(string slug);
        void Remove(string slug);
    }
}
