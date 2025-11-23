using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.Accounts.DTOs;

namespace BusinessLogic.Accounts
{
    public interface ILedgerService
    {
        // Posting operations
        Task<PostVoucherToLedgerDto> PostVoucherToLedgerAsync(Guid voucherId, Guid postedBy);
        Task<bool> HasLedgerEntriesAsync(Guid voucherId);
        
        // Ledger statement
        Task<LedgerStatementDto> GetLedgerStatementAsync(Guid accountId, DateTime fromDate, DateTime toDate);
        
        // Reports
        Task<TrialBalanceDto> GetTrialBalanceAsync(Guid businessId, DateTime fromDate, DateTime toDate);
        Task<ProfitAndLossDto> GetProfitAndLossAsync(Guid businessId, DateTime fromDate, DateTime toDate);
        
        // Reconciliation
        Task<ReconciliationCheckDto> GetReconciliationCheckAsync(Guid businessId, DateTime fromDate, DateTime toDate);
    }
}
