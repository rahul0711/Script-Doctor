using System.Threading.Tasks;

namespace Pharma_Script.Services.Interfaces
{
    public class RazorpayOrder
    {
        public string OrderId { get; set; } = string.Empty;
        public long AmountPaise { get; set; }
        public string Currency { get; set; } = "INR";
        public string KeyId { get; set; } = string.Empty;
    }

    public class RazorpayVerificationResult
    {
        public bool IsValid { get; set; }
        public string? FailureReason { get; set; }
        public string? Status { get; set; }
        public long AmountPaise { get; set; }
        public string? PaymentMethod { get; set; }
    }

    // Every call takes the KeyId/KeySecret explicitly rather than baking them in at construction
    // time - the platform's single Razorpay account (Services/Implementations/PlatformPaymentSettings)
    // is the only caller today, but the service itself stays gateway-account-agnostic.
    public interface IPaymentService
    {
        Task<RazorpayOrder> CreateOrderAsync(string keyId, string keySecret, decimal amountRupees, string receipt);
        Task<RazorpayVerificationResult> VerifyAndFetchAsync(string keyId, string keySecret, string orderId, string paymentId, string signature);
    }
}
