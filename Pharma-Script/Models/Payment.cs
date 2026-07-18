using System;

namespace Pharma_Script.Models
{
    public class Payment
    {
        public int PaymentID { get; set; }
        public int? AppointmentID { get; set; }
        public int OrganizationID { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "Cash"; // Cash, UPI, Credit Card, Debit Card, Net Banking, Razorpay
        public string? TransactionReference { get; set; }
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed, Refunded
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Currency { get; set; } = "INR";
        public string? RazorpayOrderId { get; set; }
        public string? RazorpaySignature { get; set; }

        // Marketplace settlement fields - the platform is the merchant of record, so every
        // captured payment is split at capture time into the platform's cut and the amount
        // owed to the organization (see PublicBookingController).
        public decimal? PlatformCommission { get; set; }
        public decimal? OrganizationAmount { get; set; }
        public string RefundStatus { get; set; } = "None"; // None, Requested, Refunded

        // Joined properties
        public string? PatientName { get; set; }
        public string? DoctorName { get; set; }
        public DateTime? AppointmentDate { get; set; }
    }
}
