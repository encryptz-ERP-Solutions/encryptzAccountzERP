using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Accounts.DTOs;
using Entities.Accounts;
using Repository.Accounts;
using Repository.Core.Interface;
using Infrastructure;
using Npgsql;

namespace BusinessLogic.Accounts
{
    public class LedgerService : ILedgerService
    {
        private readonly ILedgerRepository _ledgerRepository;
        private readonly IVoucherRepository _voucherRepository;
        private readonly IChartOfAccountRepository _chartOfAccountRepository;
        private readonly IBusinessRepository _businessRepository;
        private readonly CoreSQLDbHelper _sqlHelper;
        private readonly IMapper _mapper;

        public LedgerService(
            ILedgerRepository ledgerRepository,
            IVoucherRepository voucherRepository,
            IChartOfAccountRepository chartOfAccountRepository,
            IBusinessRepository businessRepository,
            CoreSQLDbHelper sqlHelper,
            IMapper mapper)
        {
            _ledgerRepository = ledgerRepository;
            _voucherRepository = voucherRepository;
            _chartOfAccountRepository = chartOfAccountRepository;
            _businessRepository = businessRepository;
            _sqlHelper = sqlHelper;
            _mapper = mapper;
        }

        public async Task<PostVoucherToLedgerDto> PostVoucherToLedgerAsync(Guid voucherId, Guid postedBy)
        {
            // Use a database transaction for atomicity
            using (var connection = _sqlHelper.GetConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Check if voucher exists
                        var voucher = await _voucherRepository.GetByIdAsync(voucherId);
                        if (voucher == null)
                        {
                            return new PostVoucherToLedgerDto
                            {
                                VoucherID = voucherId,
                                Success = false,
                                Message = "Voucher not found",
                                EntriesCreated = 0
                            };
                        }

                        // Check if voucher is already posted (idempotency check)
                        var hasExistingEntries = await _ledgerRepository.HasEntriesForVoucherAsync(voucherId);
                        if (hasExistingEntries)
                        {
                            var existingEntries = await _ledgerRepository.GetByVoucherIdAsync(voucherId);
                            return new PostVoucherToLedgerDto
                            {
                                VoucherID = voucherId,
                                Success = true,
                                Message = "Voucher already posted to ledger (idempotent operation)",
                                EntriesCreated = existingEntries.Count(),
                                EntryIDs = existingEntries.Select(e => e.EntryID).ToList()
                            };
                        }

                        // Validate voucher is in posted status
                        if (voucher.Status != "posted")
                        {
                            return new PostVoucherToLedgerDto
                            {
                                VoucherID = voucherId,
                                Success = false,
                                Message = $"Voucher must be in 'posted' status. Current status: {voucher.Status}",
                                EntriesCreated = 0
                            };
                        }

                        // Validate voucher has lines
                        if (voucher.VoucherLines == null || !voucher.VoucherLines.Any())
                        {
                            return new PostVoucherToLedgerDto
                            {
                                VoucherID = voucherId,
                                Success = false,
                                Message = "Voucher has no lines to post",
                                EntriesCreated = 0
                            };
                        }

                        // Generate ledger entries from voucher lines
                        var ledgerEntries = new List<LedgerEntry>();
                        
                        foreach (var line in voucher.VoucherLines)
                        {
                            // Skip lines with zero amounts
                            if (line.DebitAmount == 0 && line.CreditAmount == 0)
                                continue;

                            var ledgerEntry = new LedgerEntry
                            {
                                EntryID = Guid.NewGuid(),
                                BusinessID = voucher.BusinessID,
                                VoucherID = voucher.VoucherID,
                                LineID = line.LineID,
                                EntryDate = voucher.VoucherDate,
                                AccountID = line.AccountID,
                                DebitAmount = line.DebitAmount,
                                CreditAmount = line.CreditAmount,
                                Currency = voucher.Currency,
                                ExchangeRate = voucher.ExchangeRate,
                                BaseDebitAmount = line.DebitAmount * voucher.ExchangeRate,
                                BaseCreditAmount = line.CreditAmount * voucher.ExchangeRate,
                                CostCenter = line.CostCenter ?? voucher.CostCenter,
                                Project = line.Project ?? voucher.Project,
                                Narration = voucher.Narration,
                                IsOpeningBalance = false,
                                ReconciliationStatus = "unreconciled",
                                CreatedAt = DateTime.UtcNow
                            };

                            ledgerEntries.Add(ledgerEntry);
                        }

                        // Validate double-entry bookkeeping: total debits = total credits
                        var totalDebits = ledgerEntries.Sum(e => e.DebitAmount);
                        var totalCredits = ledgerEntries.Sum(e => e.CreditAmount);
                        
                        if (Math.Abs(totalDebits - totalCredits) > 0.01m) // Allow for small rounding differences
                        {
                            await transaction.RollbackAsync();
                            return new PostVoucherToLedgerDto
                            {
                                VoucherID = voucherId,
                                Success = false,
                                Message = $"Ledger entries do not balance. Total Debits: {totalDebits}, Total Credits: {totalCredits}, Difference: {totalDebits - totalCredits}",
                                EntriesCreated = 0
                            };
                        }

                        // Create ledger entries in batch
                        var createdEntries = await _ledgerRepository.CreateBatchAsync(ledgerEntries);

                        // Commit transaction
                        await transaction.CommitAsync();

                        return new PostVoucherToLedgerDto
                        {
                            VoucherID = voucherId,
                            Success = true,
                            Message = $"Successfully posted {ledgerEntries.Count} ledger entries",
                            EntriesCreated = ledgerEntries.Count,
                            EntryIDs = createdEntries.Select(e => e.EntryID).ToList()
                        };
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return new PostVoucherToLedgerDto
                        {
                            VoucherID = voucherId,
                            Success = false,
                            Message = $"Error posting voucher to ledger: {ex.Message}",
                            EntriesCreated = 0
                        };
                    }
                }
            }
        }

        public async Task<bool> HasLedgerEntriesAsync(Guid voucherId)
        {
            return await _ledgerRepository.HasEntriesForVoucherAsync(voucherId);
        }

        public async Task<LedgerStatementDto> GetLedgerStatementAsync(Guid accountId, DateTime fromDate, DateTime toDate)
        {
            // Get account details
            var account = await _chartOfAccountRepository.GetChartOfAccountByIdAsync(accountId);
            if (account == null)
                throw new ArgumentException("Account not found");

            // Get opening balance (entries before fromDate)
            var openingBalance = await _ledgerRepository.GetAccountBalanceAsync(accountId, fromDate.AddDays(-1));
            
            // Get entries for the period
            var entries = await _ledgerRepository.GetByAccountIdAsync(accountId, fromDate, toDate);
            
            // Calculate totals
            var totalDebits = entries.Sum(e => e.DebitAmount);
            var totalCredits = entries.Sum(e => e.CreditAmount);
            var closingBalance = openingBalance + totalDebits - totalCredits;

            // Determine balance type based on account type
            var openingBalanceType = DetermineBalanceType(account.AccountType, openingBalance);
            var closingBalanceType = DetermineBalanceType(account.AccountType, closingBalance);

            var statement = new LedgerStatementDto
            {
                AccountID = accountId,
                AccountCode = account.AccountCode,
                AccountName = account.AccountName,
                AccountType = account.AccountType,
                FromDate = fromDate,
                ToDate = toDate,
                OpeningBalance = Math.Abs(openingBalance),
                OpeningBalanceType = openingBalanceType,
                Entries = _mapper.Map<List<LedgerEntryDto>>(entries),
                TotalDebits = totalDebits,
                TotalCredits = totalCredits,
                ClosingBalance = Math.Abs(closingBalance),
                ClosingBalanceType = closingBalanceType
            };

            return statement;
        }

        public async Task<TrialBalanceDto> GetTrialBalanceAsync(Guid businessId, DateTime fromDate, DateTime toDate)
        {
            // Get business details
            var business = await _businessRepository.GetByIdAsync(businessId);
            if (business == null)
                throw new ArgumentException("Business not found");

            // Get all accounts for the business
            var accounts = await _chartOfAccountRepository.GetAllByBusinessIdAsync(businessId);
            var activeAccounts = accounts.Where(a => a.IsActive && !a.IsGroup).ToList();

            // Get opening balances (as of day before fromDate)
            var openingBalances = await _ledgerRepository.GetAccountBalancesAsync(businessId, fromDate.AddDays(-1));

            // Get period entries
            var periodEntries = await _ledgerRepository.GetByBusinessIdAsync(businessId, fromDate, toDate);
            var periodTotals = periodEntries
                .GroupBy(e => e.AccountID)
                .ToDictionary(
                    g => g.Key,
                    g => (debits: g.Sum(e => e.DebitAmount), credits: g.Sum(e => e.CreditAmount))
                );

            var trialBalanceItems = new List<TrialBalanceItemDto>();
            decimal totalOpeningDebit = 0, totalOpeningCredit = 0;
            decimal totalPeriodDebit = 0, totalPeriodCredit = 0;
            decimal totalClosingDebit = 0, totalClosingCredit = 0;

            foreach (var account in activeAccounts.OrderBy(a => a.AccountCode))
            {
                var openingBalance = openingBalances.ContainsKey(account.AccountID) 
                    ? openingBalances[account.AccountID] 
                    : 0;

                var periodDebit = periodTotals.ContainsKey(account.AccountID) 
                    ? periodTotals[account.AccountID].debits 
                    : 0;
                
                var periodCredit = periodTotals.ContainsKey(account.AccountID) 
                    ? periodTotals[account.AccountID].credits 
                    : 0;

                var closingBalance = openingBalance + periodDebit - periodCredit;

                // Determine opening balance side
                var openingDebit = openingBalance >= 0 ? openingBalance : 0;
                var openingCredit = openingBalance < 0 ? Math.Abs(openingBalance) : 0;

                // Determine closing balance side
                var closingDebit = closingBalance >= 0 ? closingBalance : 0;
                var closingCredit = closingBalance < 0 ? Math.Abs(closingBalance) : 0;

                // Only include accounts with activity or non-zero balances
                if (openingDebit != 0 || openingCredit != 0 || 
                    periodDebit != 0 || periodCredit != 0 || 
                    closingDebit != 0 || closingCredit != 0)
                {
                    trialBalanceItems.Add(new TrialBalanceItemDto
                    {
                        AccountID = account.AccountID,
                        AccountCode = account.AccountCode,
                        AccountName = account.AccountName,
                        AccountType = account.AccountType,
                        OpeningDebit = openingDebit,
                        OpeningCredit = openingCredit,
                        PeriodDebit = periodDebit,
                        PeriodCredit = periodCredit,
                        ClosingDebit = closingDebit,
                        ClosingCredit = closingCredit
                    });

                    totalOpeningDebit += openingDebit;
                    totalOpeningCredit += openingCredit;
                    totalPeriodDebit += periodDebit;
                    totalPeriodCredit += periodCredit;
                    totalClosingDebit += closingDebit;
                    totalClosingCredit += closingCredit;
                }
            }

            var isBalanced = Math.Abs(totalClosingDebit - totalClosingCredit) < 0.01m;
            
            return new TrialBalanceDto
            {
                BusinessID = businessId,
                BusinessName = business.BusinessName,
                FromDate = fromDate,
                ToDate = toDate,
                Accounts = trialBalanceItems,
                TotalOpeningDebit = totalOpeningDebit,
                TotalOpeningCredit = totalOpeningCredit,
                TotalPeriodDebit = totalPeriodDebit,
                TotalPeriodCredit = totalPeriodCredit,
                TotalClosingDebit = totalClosingDebit,
                TotalClosingCredit = totalClosingCredit,
                IsBalanced = isBalanced,
                Difference = totalClosingDebit - totalClosingCredit
            };
        }

        public async Task<ProfitAndLossDto> GetProfitAndLossAsync(Guid businessId, DateTime fromDate, DateTime toDate)
        {
            // Get business details
            var business = await _businessRepository.GetByIdAsync(businessId);
            if (business == null)
                throw new ArgumentException("Business not found");

            // Get all Revenue and Expense accounts
            var accounts = await _chartOfAccountRepository.GetAllByBusinessIdAsync(businessId);
            var incomeAccounts = accounts.Where(a => a.IsActive && !a.IsGroup && a.AccountType == "Revenue").ToList();
            var expenseAccounts = accounts.Where(a => a.IsActive && !a.IsGroup && a.AccountType == "Expense").ToList();

            // Get period entries
            var periodEntries = await _ledgerRepository.GetByBusinessIdAsync(businessId, fromDate, toDate);
            var accountTotals = periodEntries
                .GroupBy(e => e.AccountID)
                .ToDictionary(
                    g => g.Key,
                    g => (debits: g.Sum(e => e.DebitAmount), credits: g.Sum(e => e.CreditAmount))
                );

            var incomeItems = new List<ProfitAndLossItemDto>();
            var expenseItems = new List<ProfitAndLossItemDto>();
            decimal totalIncome = 0;
            decimal totalExpenses = 0;

            // Process income accounts (Revenue = Credit side)
            foreach (var account in incomeAccounts.OrderBy(a => a.AccountCode))
            {
                if (accountTotals.ContainsKey(account.AccountID))
                {
                    var (debits, credits) = accountTotals[account.AccountID];
                    var netAmount = credits - debits; // Revenue increases on credit side
                    
                    if (netAmount != 0)
                    {
                        incomeItems.Add(new ProfitAndLossItemDto
                        {
                            AccountID = account.AccountID,
                            AccountCode = account.AccountCode,
                            AccountName = account.AccountName,
                            AccountCategory = "Income",
                            Amount = netAmount
                        });
                        totalIncome += netAmount;
                    }
                }
            }

            // Process expense accounts (Expense = Debit side)
            foreach (var account in expenseAccounts.OrderBy(a => a.AccountCode))
            {
                if (accountTotals.ContainsKey(account.AccountID))
                {
                    var (debits, credits) = accountTotals[account.AccountID];
                    var netAmount = debits - credits; // Expenses increase on debit side
                    
                    if (netAmount != 0)
                    {
                        expenseItems.Add(new ProfitAndLossItemDto
                        {
                            AccountID = account.AccountID,
                            AccountCode = account.AccountCode,
                            AccountName = account.AccountName,
                            AccountCategory = "Expense",
                            Amount = netAmount
                        });
                        totalExpenses += netAmount;
                    }
                }
            }

            var netProfit = totalIncome - totalExpenses;

            return new ProfitAndLossDto
            {
                BusinessID = businessId,
                BusinessName = business.BusinessName,
                FromDate = fromDate,
                ToDate = toDate,
                IncomeAccounts = incomeItems,
                ExpenseAccounts = expenseItems,
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                NetProfit = netProfit,
                IsProfitable = netProfit >= 0
            };
        }

        public async Task<ReconciliationCheckDto> GetReconciliationCheckAsync(Guid businessId, DateTime fromDate, DateTime toDate)
        {
            // Get business details
            var business = await _businessRepository.GetByIdAsync(businessId);
            if (business == null)
                throw new ArgumentException("Business not found");

            // Get totals for the period
            var (totalDebits, totalCredits) = await _ledgerRepository.GetTotalsForPeriodAsync(businessId, fromDate, toDate);
            var difference = totalDebits - totalCredits;
            var isBalanced = Math.Abs(difference) < 0.01m;

            // Get entry count
            var allEntries = await _ledgerRepository.GetByBusinessIdAsync(businessId, fromDate, toDate);
            var totalEntries = allEntries.Count();
            var voucherIds = allEntries.Select(e => e.VoucherID).Distinct();
            var totalVouchers = voucherIds.Count();

            // Check for unbalanced vouchers
            var unbalancedVouchers = new List<VoucherBalanceDto>();
            
            foreach (var voucherId in voucherIds)
            {
                var voucherEntries = allEntries.Where(e => e.VoucherID == voucherId).ToList();
                var voucherDebits = voucherEntries.Sum(e => e.DebitAmount);
                var voucherCredits = voucherEntries.Sum(e => e.CreditAmount);
                var voucherDifference = voucherDebits - voucherCredits;

                if (Math.Abs(voucherDifference) > 0.01m)
                {
                    var voucher = await _voucherRepository.GetByIdAsync(voucherId);
                    if (voucher != null)
                    {
                        unbalancedVouchers.Add(new VoucherBalanceDto
                        {
                            VoucherID = voucherId,
                            VoucherNumber = voucher.VoucherNumber,
                            VoucherType = voucher.VoucherType,
                            VoucherDate = voucher.VoucherDate,
                            TotalDebits = voucherDebits,
                            TotalCredits = voucherCredits,
                            Difference = voucherDifference
                        });
                    }
                }
            }

            return new ReconciliationCheckDto
            {
                BusinessID = businessId,
                BusinessName = business.BusinessName,
                FromDate = fromDate,
                ToDate = toDate,
                TotalDebits = totalDebits,
                TotalCredits = totalCredits,
                Difference = difference,
                IsBalanced = isBalanced,
                TotalEntries = totalEntries,
                TotalVouchers = totalVouchers,
                UnbalancedVouchers = unbalancedVouchers
            };
        }

        private string DetermineBalanceType(string accountType, decimal balance)
        {
            // Asset and Expense accounts have normal debit balance
            // Liability, Equity, and Revenue accounts have normal credit balance
            
            if (accountType == "Asset" || accountType == "Expense")
            {
                return balance >= 0 ? "Dr" : "Cr";
            }
            else // Liability, Equity, Revenue
            {
                return balance >= 0 ? "Dr" : "Cr";
            }
        }
    }
}
