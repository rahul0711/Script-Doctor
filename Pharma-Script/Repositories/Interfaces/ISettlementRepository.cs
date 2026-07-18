using Pharma_Script.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pharma_Script.Repositories.Interfaces
{
    public interface ISettlementRepository : IRepository<OrganizationSettlement>
    {
        Task<IEnumerable<OrganizationSettlement>> SearchAndPaginateAsync(int? orgId, string? status, int page, int pageSize);
        Task<int> GetSearchCountAsync(int? orgId, string? status);
        Task<OrganizationSettlement?> GetByIdScopedAsync(int settlementId, int? orgId);
        Task<IEnumerable<SettlementTransaction>> GetTransactionsBySettlementIdAsync(int settlementId);

        // Sums every unsettled ("Paid" payment with no SettlementTransaction row yet) payment for
        // the organization into one new settlement. Returns null when there is nothing to settle.
        Task<OrganizationSettlement?> GenerateSettlementAsync(int orgId);
        Task<bool> MarkPaidAsync(int settlementId, string? paymentReference, string? notes, int settledByUserId);

        Task<int> GetPendingCountAsync(int? orgId);
        Task<int> GetCompletedCountAsync(int? orgId);
        Task<decimal> GetPendingTotalAsync(int orgId);
        Task<OrganizationSettlement?> GetLastPaidAsync(int orgId);
        Task<IEnumerable<OrganizationSettlement>> GetRecentAsync(int? orgId, int count);
    }
}
