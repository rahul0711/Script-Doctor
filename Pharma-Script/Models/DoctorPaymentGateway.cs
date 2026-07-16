using System;

namespace Pharma_Script.Models
{
    public class DoctorPaymentGateway
    {
        public int DoctorPaymentGatewayID { get; set; }
        public int OrganizationID { get; set; }
        public int DoctorID { get; set; }
        public string PaymentProvider { get; set; } = "Razorpay";
        public string KeyID { get; set; } = string.Empty;
        public string KeySecret { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
