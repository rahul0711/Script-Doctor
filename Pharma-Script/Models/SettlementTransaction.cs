namespace Pharma_Script.Models
{
    public class SettlementTransaction
    {
        public int SettlementTransactionID { get; set; }
        public int SettlementID { get; set; }
        public int PaymentID { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal NetAmount { get; set; }

        // Joined properties
        public string? PatientName { get; set; }
        public string? DoctorName { get; set; }
        public System.DateTime? PaidAt { get; set; }
    }
}
