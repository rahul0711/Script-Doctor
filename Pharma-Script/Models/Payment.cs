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

        // Joined properties
        public string? PatientName { get; set; }
        public string? DoctorName { get; set; }
        public DateTime? AppointmentDate { get; set; }
    }
}
