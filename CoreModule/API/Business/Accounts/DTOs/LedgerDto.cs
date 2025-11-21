using System;
using System.Collections.Generic;

namespace BusinessLogic.Accounts.DTOs
{
    public class LedgerEntryDto
    {
        public Guid EntryID { get; set; }
        public Guid BusinessID { get; set; }
        public Guid VoucherID { get; set; }
        public Guid? LineID { get; set; }
        public DateTime EntryDate { get; set; }
        public Guid AccountID { get; set; }
        public string AccountName { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public string Currency { get; set; }
        public decimal ExchangeRate { get; set; }
        public string? CostCenter { get; set; }
        public string? Project { get; set; }
        public string? Narration { get; set; }
        public bool IsOpeningBalance { get; set; }
        public string ReconciliationStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class LedgerStatementDto
    {
        public Guid AccountID { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal OpeningBalance { get; set; }
        public string OpeningBalanceType { get; set; } // "Dr" or "Cr"
        public List<LedgerEntryDto> Entries { get; set; } = new List<LedgerEntryDto>();
        public decimal TotalDebits { get; set; }
        public decimal TotalCredits { get; set; }
        public decimal ClosingBalance { get; set; }
        public string ClosingBalanceType { get; set; } // "Dr" or "Cr"
    }

    public class TrialBalanceItemDto
    {
        public Guid AccountID { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; }
        public int AccountTypeID { get; set; }
        public decimal OpeningDebit { get; set; }
        public decimal OpeningCredit { get; set; }
        public decimal PeriodDebit { get; set; }
        public decimal PeriodCredit { get; set; }
        public decimal ClosingDebit { get; set; }
        public decimal ClosingCredit { get; set; }
    }

    public class TrialBalanceDto
    {
        public Guid BusinessID { get; set; }
        public string BusinessName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<TrialBalanceItemDto> Accounts { get; set; } = new List<TrialBalanceItemDto>();
        public decimal TotalOpeningDebit { get; set; }
        public decimal TotalOpeningCredit { get; set; }
        public decimal TotalPeriodDebit { get; set; }
        public decimal TotalPeriodCredit { get; set; }
        public decimal TotalClosingDebit { get; set; }
        public decimal TotalClosingCredit { get; set; }
        public bool IsBalanced { get; set; }
        public decimal Difference { get; set; }
    }

    public class ProfitAndLossItemDto
    {
        public Guid AccountID { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public string AccountCategory { get; set; } // "Income" or "Expense"
        public decimal Amount { get; set; }
    }

    public class ProfitAndLossDto
    {
        public Guid BusinessID { get; set; }
        public string BusinessName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<ProfitAndLossItemDto> IncomeAccounts { get; set; } = new List<ProfitAndLossItemDto>();
        public List<ProfitAndLossItemDto> ExpenseAccounts { get; set; } = new List<ProfitAndLossItemDto>();
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public bool IsProfitable { get; set; }
    }

    public class ReconciliationCheckDto
    {
        public Guid BusinessID { get; set; }
        public string BusinessName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalDebits { get; set; }
        public decimal TotalCredits { get; set; }
        public decimal Difference { get; set; }
        public bool IsBalanced { get; set; }
        public int TotalEntries { get; set; }
        public int TotalVouchers { get; set; }
        public List<VoucherBalanceDto> UnbalancedVouchers { get; set; } = new List<VoucherBalanceDto>();
    }

    public class VoucherBalanceDto
    {
        public Guid VoucherID { get; set; }
        public string VoucherNumber { get; set; }
        public string VoucherType { get; set; }
        public DateTime VoucherDate { get; set; }
        public decimal TotalDebits { get; set; }
        public decimal TotalCredits { get; set; }
        public decimal Difference { get; set; }
    }

    public class PostVoucherToLedgerDto
    {
        public Guid VoucherID { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public int EntriesCreated { get; set; }
        public List<Guid> EntryIDs { get; set; } = new List<Guid>();
    }
}
