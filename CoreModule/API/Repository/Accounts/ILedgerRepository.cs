using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Accounts;

namespace Repository.Accounts
{
    public interface ILedgerRepository
    {
        Task<LedgerEntry> GetByIdAsync(Guid entryId);
        Task<IEnumerable<LedgerEntry>> GetByBusinessIdAsync(Guid businessId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<LedgerEntry>> GetByAccountIdAsync(Guid accountId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<LedgerEntry>> GetByVoucherIdAsync(Guid voucherId);
        Task<LedgerEntry> CreateAsync(LedgerEntry entry);
        Task<IEnumerable<LedgerEntry>> CreateBatchAsync(IEnumerable<LedgerEntry> entries);
        Task<bool> HasEntriesForVoucherAsync(Guid voucherId);
        Task<decimal> GetAccountBalanceAsync(Guid accountId, DateTime? asOfDate = null);
        Task<Dictionary<Guid, decimal>> GetAccountBalancesAsync(Guid businessId, DateTime? asOfDate = null);
        Task<(decimal totalDebits, decimal totalCredits)> GetTotalsForPeriodAsync(Guid businessId, DateTime fromDate, DateTime toDate);
        Task<IEnumerable<LedgerEntry>> GetUnreconciledEntriesAsync(Guid accountId);
        Task<bool> DeleteEntriesForVoucherAsync(Guid voucherId);
    }
}
