using Pharma_Script.Repositories.Interfaces;
using Pharma_Script.Services.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Pharma_Script.Services.Implementations
{
    // Centralized, in-memory lookup of active organization slugs.
    // Backs the route constraint that decides whether a bare "/segment" URL
    // belongs to a tenant public website, without hitting the database per request.
    public class OrganizationSlugCache : IOrganizationSlugCache
    {
        private readonly ConcurrentDictionary<string, byte> _activeSlugs = new(StringComparer.OrdinalIgnoreCase);

        public async Task RefreshAsync(IUnitOfWork uow)
        {
            var orgs = await uow.Organizations.GetAllAsync();
            var slugs = orgs
                .Where(o => o.IsActive && !string.IsNullOrWhiteSpace(o.OrganizationSlug))
                .Select(o => o.OrganizationSlug!);

            _activeSlugs.Clear();
            foreach (var slug in slugs)
            {
                _activeSlugs.TryAdd(slug, 0);
            }
        }

        public bool IsActiveSlug(string slug)
        {
            return !string.IsNullOrWhiteSpace(slug) && _activeSlugs.ContainsKey(slug);
        }

        public void SetActive(string slug)
        {
            if (!string.IsNullOrWhiteSpace(slug))
            {
                _activeSlugs.TryAdd(slug, 0);
            }
        }

        public void Remove(string slug)
        {
            if (!string.IsNullOrWhiteSpace(slug))
            {
                _activeSlugs.TryRemove(slug, out _);
            }
        }
    }
}
