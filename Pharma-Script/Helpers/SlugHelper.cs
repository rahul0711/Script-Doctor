using System.Text;
using System.Text.RegularExpressions;

namespace Pharma_Script.Helpers
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            var lowered = value.Trim().ToLowerInvariant();
            var sb = new StringBuilder();
            foreach (var c in lowered)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                }
                else if (c == ' ' || c == '-' || c == '_')
                {
                    sb.Append('-');
                }
            }

            var slug = Regex.Replace(sb.ToString(), "-{2,}", "-").Trim('-');
            return slug;
        }
    }
}
