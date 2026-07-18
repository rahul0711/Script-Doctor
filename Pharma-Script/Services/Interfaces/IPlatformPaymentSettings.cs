namespace Pharma_Script.Services.Interfaces
{
    public interface IPlatformPaymentSettings
    {
        string KeyId { get; }
        string KeySecret { get; }
        decimal CommissionPercent { get; }
        bool IsConfigured { get; }
    }
}
