using Pharma_Script.Services.Interfaces;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pharma_Script.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;

        public PaymentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://api.razorpay.com/v1/");
        }

        public async Task<RazorpayOrder> CreateOrderAsync(string keyId, string keySecret, decimal amountRupees, string receipt)
        {
            var amountPaise = (long)Math.Round(amountRupees * 100, MidpointRounding.AwayFromZero);

            using var request = new HttpRequestMessage(HttpMethod.Post, "orders")
            {
                Content = JsonContent.Create(new
                {
                    amount = amountPaise,
                    currency = "INR",
                    receipt
                })
            };
            request.Headers.Authorization = BuildAuthHeader(keyId, keySecret);

            var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Razorpay order creation failed: {body}");
            }

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            return new RazorpayOrder
            {
                OrderId = root.GetProperty("id").GetString() ?? string.Empty,
                AmountPaise = root.GetProperty("amount").GetInt64(),
                Currency = root.GetProperty("currency").GetString() ?? "INR",
                KeyId = keyId
            };
        }

        public async Task<RazorpayVerificationResult> VerifyAndFetchAsync(string keyId, string keySecret, string orderId, string paymentId, string signature)
        {
            if (string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(paymentId) || string.IsNullOrWhiteSpace(signature))
            {
                return new RazorpayVerificationResult { IsValid = false, FailureReason = "Missing payment verification details." };
            }

            if (!SignatureMatches(keySecret, orderId, paymentId, signature))
            {
                return new RazorpayVerificationResult { IsValid = false, FailureReason = "Payment signature verification failed." };
            }

            // Defense-in-depth: confirm the payment is actually captured server-to-server,
            // and pull the real payment method for accurate reporting.
            using var request = new HttpRequestMessage(HttpMethod.Get, $"payments/{paymentId}");
            request.Headers.Authorization = BuildAuthHeader(keyId, keySecret);

            var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return new RazorpayVerificationResult { IsValid = false, FailureReason = $"Could not confirm payment with Razorpay: {body}" };
            }

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var status = root.GetProperty("status").GetString();
            var amountPaise = root.GetProperty("amount").GetInt64();
            var method = root.TryGetProperty("method", out var methodEl) ? methodEl.GetString() : null;

            if (!string.Equals(status, "captured", StringComparison.OrdinalIgnoreCase))
            {
                return new RazorpayVerificationResult { IsValid = false, FailureReason = $"Payment is not captured (status: {status}).", Status = status };
            }

            string mappedMethod = method switch
            {
                "upi" => "UPI",
                "netbanking" => "Net Banking",
                "card" => root.TryGetProperty("card", out var card) && card.TryGetProperty("type", out var cardType) && cardType.GetString() == "debit"
                    ? "Debit Card"
                    : "Credit Card",
                _ => "Razorpay"
            };

            return new RazorpayVerificationResult
            {
                IsValid = true,
                Status = status,
                AmountPaise = amountPaise,
                PaymentMethod = mappedMethod
            };
        }

        private static AuthenticationHeaderValue BuildAuthHeader(string keyId, string keySecret)
        {
            var basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{keyId}:{keySecret}"));
            return new AuthenticationHeaderValue("Basic", basicAuth);
        }

        private bool SignatureMatches(string keySecret, string orderId, string paymentId, string signature)
        {
            var payload = $"{orderId}|{paymentId}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(keySecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var expected = Convert.ToHexString(hash).ToLowerInvariant();

            var expectedBytes = Encoding.UTF8.GetBytes(expected);
            var actualBytes = Encoding.UTF8.GetBytes(signature.ToLowerInvariant());
            if (expectedBytes.Length != actualBytes.Length)
            {
                return false;
            }
            return CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
        }
    }
}
