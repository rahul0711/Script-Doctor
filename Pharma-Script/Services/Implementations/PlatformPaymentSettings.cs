using Microsoft.Extensions.Configuration;
using Pharma_Script.Services.Interfaces;

namespace Pharma_Script.Services.Implementations
{
    // The platform owns a single Razorpay account - credentials live in appsettings.json
    // (same pattern as the "Smtp" section in EmailService) rather than the database, since
    // no organization or doctor is ever allowed to configure their own gateway anymore.
    public class PlatformPaymentSettings : IPlatformPaymentSettings
    {
        public string KeyId { get; }
        public string KeySecret { get; }
        public decimal CommissionPercent { get; }
        public bool IsConfigured => !string.IsNullOrWhiteSpace(KeyId) && !string.IsNullOrWhiteSpace(KeySecret);

        public PlatformPaymentSettings(IConfiguration config)
        {
            var section = config.GetSection("Razorpay");
            KeyId = section["KeyId"] ?? string.Empty;
            KeySecret = section["KeySecret"] ?? string.Empty;
            CommissionPercent = decimal.TryParse(section["CommissionPercent"], out var pct) ? pct : 0m;
        }
    }
}
