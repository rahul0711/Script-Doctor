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

    // Every call takes the KeyId/KeySecret explicitly - each doctor has their own Razorpay
    // credentials (Repositories/Implementations/DoctorPaymentGatewayRepository), so there is no
    // single global key to bake into the service at construction time.
    public interface IPaymentService
    {
        Task<RazorpayOrder> CreateOrderAsync(string keyId, string keySecret, decimal amountRupees, string receipt);
        Task<RazorpayVerificationResult> VerifyAndFetchAsync(string keyId, string keySecret, string orderId, string paymentId, string signature);
    }
}
