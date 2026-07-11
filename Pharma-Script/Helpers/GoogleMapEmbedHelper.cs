using System;
using System.Text.RegularExpressions;

namespace Pharma_Script.Helpers
{
    // Accepts either a raw Google Maps embed URL or a pasted <iframe> snippet,
    // extracts just the src URL, and validates it points at Google Maps.
    // Only the validated URL is ever persisted/rendered - never the raw markup -
    // so a malicious <script> pasted into this field can never execute.
    public static class GoogleMapEmbedHelper
    {
        private static readonly Regex SrcRegex = new(@"src\s*=\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase);

        public static bool TryGetSafeEmbedUrl(string? input, out string? safeUrl)
        {
            safeUrl = null;
            if (string.IsNullOrWhiteSpace(input)) return false;

            var candidate = input.Trim();
            var match = SrcRegex.Match(candidate);
            if (match.Success)
            {
                candidate = match.Groups[1].Value;
            }

            if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
            {
                return false;
            }

            if (uri.Scheme != Uri.UriSchemeHttps)
            {
                return false;
            }

            var host = uri.Host.ToLowerInvariant();
            var isGoogleMapsHost = host == "maps.google.com"
                || host == "www.google.com"
                || host.EndsWith(".google.com");

            if (!isGoogleMapsHost || !uri.AbsolutePath.Contains("/maps", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            safeUrl = uri.ToString();
            return true;
        }
    }
}
