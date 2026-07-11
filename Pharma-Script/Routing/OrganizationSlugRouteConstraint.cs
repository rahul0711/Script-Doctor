using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Pharma_Script.Services.Interfaces;

namespace Pharma_Script.Routing
{
    // Matches a route segment only if it is a known, active organization slug.
    // Keeps the public tenant website ("/abc-hospital/...") from colliding with
    // conventional admin routes ("/Dashboard", "/Organizations", ...).
    public class OrganizationSlugRouteConstraint : IRouteConstraint
    {
        private readonly IOrganizationSlugCache _slugCache;

        public OrganizationSlugRouteConstraint(IOrganizationSlugCache slugCache)
        {
            _slugCache = slugCache;
        }

        public bool Match(
            HttpContext? httpContext,
            Microsoft.AspNetCore.Routing.IRouter? route,
            string routeKey,
            RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            if (!values.TryGetValue(routeKey, out var value) || value is null)
            {
                return false;
            }

            return _slugCache.IsActiveSlug(value.ToString() ?? string.Empty);
        }
    }
}
