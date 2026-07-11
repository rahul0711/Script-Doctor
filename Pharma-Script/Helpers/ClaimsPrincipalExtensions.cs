using System;
using System.Security.Claims;

namespace Pharma_Script.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }

        public static int? GetOrganizationId(this ClaimsPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            var claim = principal.FindFirst("OrganizationID");
            if (claim != null && int.TryParse(claim.Value, out var orgId))
            {
                return orgId;
            }
            return null;
        }

        public static string GetRoleName(this ClaimsPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            return principal.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        public static string GetFullName(this ClaimsPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            return principal.FindFirst("FullName")?.Value ?? (principal.Identity?.Name ?? "User");
        }

        public static bool IsPlatformOwner(this ClaimsPrincipal principal)
        {
            return principal.GetRoleName().Equals("Platform Owner", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsOrganizationAdmin(this ClaimsPrincipal principal)
        {
            return principal.GetRoleName().Equals("Organization Admin", StringComparison.OrdinalIgnoreCase);
        }
    }
}
