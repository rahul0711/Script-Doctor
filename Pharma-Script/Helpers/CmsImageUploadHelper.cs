using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pharma_Script.Helpers
{
    public class CmsUploadValidationException : Exception
    {
        public CmsUploadValidationException(string message) : base(message) { }
    }

    // Public-facing CMS images (logo, favicon, banners, service/gallery images) live under
    // wwwroot so they can be served directly on the tenant website without authentication.
    // Filenames are always regenerated (GUID) so client input never reaches the file path.
    public static class CmsImageUploadHelper
    {
        private static readonly string[] DefaultAllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public static async Task<string> UploadAsync(
            IFormFile file,
            string webRootPath,
            int organizationId,
            string subfolder,
            long maxSizeBytes = 5 * 1024 * 1024,
            string[]? allowedExtensions = null)
        {
            allowedExtensions ??= DefaultAllowedExtensions;

            if (file == null || file.Length == 0)
            {
                throw new CmsUploadValidationException("No file was uploaded.");
            }

            if (file.Length > maxSizeBytes)
            {
                throw new CmsUploadValidationException($"File exceeds the maximum allowed size of {maxSizeBytes / (1024 * 1024)}MB.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
            {
                throw new CmsUploadValidationException($"Unsupported file type. Allowed: {string.Join(", ", allowedExtensions)}.");
            }

            var uploadDir = Path.Combine(webRootPath, "uploads", "cms", organizationId.ToString(), subfolder);
            Directory.CreateDirectory(uploadDir);

            var safeFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadDir, safeFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/cms/{organizationId}/{subfolder}/{safeFileName}";
        }

        public static void DeleteIfExists(string webRootPath, string? relativeUrl)
        {
            if (string.IsNullOrWhiteSpace(relativeUrl)) return;

            var uploadsRoot = Path.Combine(webRootPath, "uploads", "cms");
            var fullPath = Path.GetFullPath(Path.Combine(webRootPath, relativeUrl.TrimStart('/')));

            if (fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase) && File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}
