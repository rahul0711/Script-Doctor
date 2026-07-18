using System;

namespace Pharma_Script.Models
{
    public class OrganizationSettlement
    {
        public int SettlementID { get; set; }
        public int OrganizationID { get; set; }
        public decimal TotalGrossAmount { get; set; }
        public decimal TotalCommission { get; set; }
        public decimal TotalNetAmount { get; set; }
        public string SettlementStatus { get; set; } = "Pending"; // Pending, Paid
        public DateTime GeneratedAt { get; set; }
        public DateTime? SettledAt { get; set; }
        public int? SettledByUserID { get; set; }
        public string? PaymentReference { get; set; }
        public string? Notes { get; set; }

        // Joined properties
        public string? OrganizationName { get; set; }
    }
}
