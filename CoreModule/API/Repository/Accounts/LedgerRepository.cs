using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Entities.Accounts;
using Infrastructure;
using Npgsql;

namespace Repository.Accounts
{
    public class LedgerRepository : ILedgerRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;

        public LedgerRepository(CoreSQLDbHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public async Task<LedgerEntry> GetByIdAsync(Guid entryId)
        {
            var query = @"
                SELECT le.*, coa.account_name, v.voucher_number, v.voucher_type
                FROM ledger_entries le
                LEFT JOIN chart_of_accounts coa ON le.account_id = coa.account_id
                LEFT JOIN vouchers v ON le.voucher_id = v.voucher_id
                WHERE le.entry_id = @EntryID";
            
            var parameters = new[] { new NpgsqlParameter("@EntryID", entryId) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0)
                return null;

            return MapDataRowToLedgerEntry(dataTable.Rows[0]);
        }

        public async Task<IEnumerable<LedgerEntry>> GetByBusinessIdAsync(Guid businessId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = @"
                SELECT le.*, coa.account_name, v.voucher_number, v.voucher_type
                FROM ledger_entries le
                LEFT JOIN chart_of_accounts coa ON le.account_id = coa.account_id
                LEFT JOIN vouchers v ON le.voucher_id = v.voucher_id
                WHERE le.business_id = @BusinessID";

            var paramsList = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@BusinessID", businessId)
            };

            if (fromDate.HasValue)
            {
                query += " AND le.entry_date >= @FromDate";
                paramsList.Add(new NpgsqlParameter("@FromDate", fromDate.Value));
            }

            if (toDate.HasValue)
            {
                query += " AND le.entry_date <= @ToDate";
                paramsList.Add(new NpgsqlParameter("@ToDate", toDate.Value));
            }

            query += " ORDER BY le.entry_date, le.created_at";

            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, paramsList.ToArray());
            return MapDataTableToLedgerEntries(dataTable);
        }

        public async Task<IEnumerable<LedgerEntry>> GetByAccountIdAsync(Guid accountId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = @"
                SELECT le.*, coa.account_name, v.voucher_number, v.voucher_type
                FROM ledger_entries le
                LEFT JOIN chart_of_accounts coa ON le.account_id = coa.account_id
                LEFT JOIN vouchers v ON le.voucher_id = v.voucher_id
                WHERE le.account_id = @AccountID";

            var paramsList = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@AccountID", accountId)
            };

            if (fromDate.HasValue)
            {
                query += " AND le.entry_date >= @FromDate";
                paramsList.Add(new NpgsqlParameter("@FromDate", fromDate.Value));
            }

            if (toDate.HasValue)
            {
                query += " AND le.entry_date <= @ToDate";
                paramsList.Add(new NpgsqlParameter("@ToDate", toDate.Value));
            }

            query += " ORDER BY le.entry_date, le.created_at";

            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, paramsList.ToArray());
            return MapDataTableToLedgerEntries(dataTable);
        }

        public async Task<IEnumerable<LedgerEntry>> GetByVoucherIdAsync(Guid voucherId)
        {
            var query = @"
                SELECT le.*, coa.account_name, v.voucher_number, v.voucher_type
                FROM ledger_entries le
                LEFT JOIN chart_of_accounts coa ON le.account_id = coa.account_id
                LEFT JOIN vouchers v ON le.voucher_id = v.voucher_id
                WHERE le.voucher_id = @VoucherID
                ORDER BY le.created_at";

            var parameters = new[] { new NpgsqlParameter("@VoucherID", voucherId) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            return MapDataTableToLedgerEntries(dataTable);
        }

        public async Task<LedgerEntry> CreateAsync(LedgerEntry entry)
        {
            var query = @"
                INSERT INTO ledger_entries (
                    entry_id, business_id, voucher_id, line_id, entry_date, account_id,
                    debit_amount, credit_amount, currency, exchange_rate,
                    base_debit_amount, base_credit_amount, cost_center, project,
                    reconciliation_status, is_opening_balance, narration, created_at
                )
                VALUES (
                    @EntryID, @BusinessID, @VoucherID, @LineID, @EntryDate, @AccountID,
                    @DebitAmount, @CreditAmount, @Currency, @ExchangeRate,
                    @BaseDebitAmount, @BaseCreditAmount, @CostCenter, @Project,
                    @ReconciliationStatus, @IsOpeningBalance, @Narration, @CreatedAt
                )";

            var parameters = new[]
            {
                new NpgsqlParameter("@EntryID", entry.EntryID),
                new NpgsqlParameter("@BusinessID", entry.BusinessID),
                new NpgsqlParameter("@VoucherID", entry.VoucherID),
                new NpgsqlParameter("@LineID", (object)entry.LineID ?? DBNull.Value),
                new NpgsqlParameter("@EntryDate", entry.EntryDate),
                new NpgsqlParameter("@AccountID", entry.AccountID),
                new NpgsqlParameter("@DebitAmount", entry.DebitAmount),
                new NpgsqlParameter("@CreditAmount", entry.CreditAmount),
                new NpgsqlParameter("@Currency", entry.Currency),
                new NpgsqlParameter("@ExchangeRate", entry.ExchangeRate),
                new NpgsqlParameter("@BaseDebitAmount", entry.BaseDebitAmount),
                new NpgsqlParameter("@BaseCreditAmount", entry.BaseCreditAmount),
                new NpgsqlParameter("@CostCenter", (object)entry.CostCenter ?? DBNull.Value),
                new NpgsqlParameter("@Project", (object)entry.Project ?? DBNull.Value),
                new NpgsqlParameter("@ReconciliationStatus", entry.ReconciliationStatus),
                new NpgsqlParameter("@IsOpeningBalance", entry.IsOpeningBalance),
                new NpgsqlParameter("@Narration", (object)entry.Narration ?? DBNull.Value),
                new NpgsqlParameter("@CreatedAt", entry.CreatedAt)
            };

            await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return entry;
        }

        public async Task<IEnumerable<LedgerEntry>> CreateBatchAsync(IEnumerable<LedgerEntry> entries)
        {
            var entriesList = entries.ToList();
            
            // Use a transaction for batch insert
            using (var connection = _sqlHelper.GetConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var entry in entriesList)
                        {
                            var query = @"
                                INSERT INTO ledger_entries (
                                    entry_id, business_id, voucher_id, line_id, entry_date, account_id,
                                    debit_amount, credit_amount, currency, exchange_rate,
                                    base_debit_amount, base_credit_amount, cost_center, project,
                                    reconciliation_status, is_opening_balance, narration, created_at
                                )
                                VALUES (
                                    @EntryID, @BusinessID, @VoucherID, @LineID, @EntryDate, @AccountID,
                                    @DebitAmount, @CreditAmount, @Currency, @ExchangeRate,
                                    @BaseDebitAmount, @BaseCreditAmount, @CostCenter, @Project,
                                    @ReconciliationStatus, @IsOpeningBalance, @Narration, @CreatedAt
                                )";

                            using (var cmd = new NpgsqlCommand(query, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@EntryID", entry.EntryID);
                                cmd.Parameters.AddWithValue("@BusinessID", entry.BusinessID);
                                cmd.Parameters.AddWithValue("@VoucherID", entry.VoucherID);
                                cmd.Parameters.AddWithValue("@LineID", (object)entry.LineID ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@EntryDate", entry.EntryDate);
                                cmd.Parameters.AddWithValue("@AccountID", entry.AccountID);
                                cmd.Parameters.AddWithValue("@DebitAmount", entry.DebitAmount);
                                cmd.Parameters.AddWithValue("@CreditAmount", entry.CreditAmount);
                                cmd.Parameters.AddWithValue("@Currency", entry.Currency);
                                cmd.Parameters.AddWithValue("@ExchangeRate", entry.ExchangeRate);
                                cmd.Parameters.AddWithValue("@BaseDebitAmount", entry.BaseDebitAmount);
                                cmd.Parameters.AddWithValue("@BaseCreditAmount", entry.BaseCreditAmount);
                                cmd.Parameters.AddWithValue("@CostCenter", (object)entry.CostCenter ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@Project", (object)entry.Project ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@ReconciliationStatus", entry.ReconciliationStatus);
                                cmd.Parameters.AddWithValue("@IsOpeningBalance", entry.IsOpeningBalance);
                                cmd.Parameters.AddWithValue("@Narration", (object)entry.Narration ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@CreatedAt", entry.CreatedAt);

                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }

            return entriesList;
        }

        public async Task<bool> HasEntriesForVoucherAsync(Guid voucherId)
        {
            var query = "SELECT COUNT(*) FROM ledger_entries WHERE voucher_id = @VoucherID";
            var parameters = new[] { new NpgsqlParameter("@VoucherID", voucherId) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            
            if (dataTable.Rows.Count > 0)
            {
                var count = Convert.ToInt32(dataTable.Rows[0][0]);
                return count > 0;
            }
            
            return false;
        }

        public async Task<decimal> GetAccountBalanceAsync(Guid accountId, DateTime? asOfDate = null)
        {
            var query = @"
                SELECT 
                    COALESCE(SUM(debit_amount), 0) - COALESCE(SUM(credit_amount), 0) as balance
                FROM ledger_entries
                WHERE account_id = @AccountID";

            var paramsList = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@AccountID", accountId)
            };

            if (asOfDate.HasValue)
            {
                query += " AND entry_date <= @AsOfDate";
                paramsList.Add(new NpgsqlParameter("@AsOfDate", asOfDate.Value));
            }

            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, paramsList.ToArray());
            
            if (dataTable.Rows.Count > 0 && dataTable.Rows[0]["balance"] != DBNull.Value)
                return Convert.ToDecimal(dataTable.Rows[0]["balance"]);
            
            return 0;
        }

        public async Task<Dictionary<Guid, decimal>> GetAccountBalancesAsync(Guid businessId, DateTime? asOfDate = null)
        {
            var query = @"
                SELECT 
                    account_id,
                    COALESCE(SUM(debit_amount), 0) - COALESCE(SUM(credit_amount), 0) as balance
                FROM ledger_entries
                WHERE business_id = @BusinessID";

            var paramsList = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@BusinessID", businessId)
            };

            if (asOfDate.HasValue)
            {
                query += " AND entry_date <= @AsOfDate";
                paramsList.Add(new NpgsqlParameter("@AsOfDate", asOfDate.Value));
            }

            query += " GROUP BY account_id";

            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, paramsList.ToArray());
            var balances = new Dictionary<Guid, decimal>();

            foreach (DataRow row in dataTable.Rows)
            {
                var accountId = (Guid)row["account_id"];
                var balance = Convert.ToDecimal(row["balance"]);
                balances[accountId] = balance;
            }

            return balances;
        }

        public async Task<(decimal totalDebits, decimal totalCredits)> GetTotalsForPeriodAsync(Guid businessId, DateTime fromDate, DateTime toDate)
        {
            var query = @"
                SELECT 
                    COALESCE(SUM(debit_amount), 0) as total_debits,
                    COALESCE(SUM(credit_amount), 0) as total_credits
                FROM ledger_entries
                WHERE business_id = @BusinessID
                  AND entry_date >= @FromDate
                  AND entry_date <= @ToDate";

            var parameters = new[]
            {
                new NpgsqlParameter("@BusinessID", businessId),
                new NpgsqlParameter("@FromDate", fromDate),
                new NpgsqlParameter("@ToDate", toDate)
            };

            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            
            if (dataTable.Rows.Count > 0)
            {
                var totalDebits = Convert.ToDecimal(dataTable.Rows[0]["total_debits"]);
                var totalCredits = Convert.ToDecimal(dataTable.Rows[0]["total_credits"]);
                return (totalDebits, totalCredits);
            }
            
            return (0, 0);
        }

        public async Task<IEnumerable<LedgerEntry>> GetUnreconciledEntriesAsync(Guid accountId)
        {
            var query = @"
                SELECT le.*, coa.account_name, v.voucher_number, v.voucher_type
                FROM ledger_entries le
                LEFT JOIN chart_of_accounts coa ON le.account_id = coa.account_id
                LEFT JOIN vouchers v ON le.voucher_id = v.voucher_id
                WHERE le.account_id = @AccountID
                  AND le.reconciliation_status = 'unreconciled'
                ORDER BY le.entry_date";

            var parameters = new[] { new NpgsqlParameter("@AccountID", accountId) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            return MapDataTableToLedgerEntries(dataTable);
        }

        public async Task<bool> DeleteEntriesForVoucherAsync(Guid voucherId)
        {
            var query = "DELETE FROM ledger_entries WHERE voucher_id = @VoucherID";
            var parameters = new[] { new NpgsqlParameter("@VoucherID", voucherId) };
            var rowsAffected = await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        private LedgerEntry MapDataRowToLedgerEntry(DataRow row)
        {
            return new LedgerEntry
            {
                EntryID = (Guid)row["entry_id"],
                BusinessID = (Guid)row["business_id"],
                VoucherID = (Guid)row["voucher_id"],
                LineID = row["line_id"] != DBNull.Value ? (Guid?)row["line_id"] : null,
                EntryDate = Convert.ToDateTime(row["entry_date"]),
                AccountID = (Guid)row["account_id"],
                DebitAmount = Convert.ToDecimal(row["debit_amount"]),
                CreditAmount = Convert.ToDecimal(row["credit_amount"]),
                Currency = row["currency"].ToString(),
                ExchangeRate = Convert.ToDecimal(row["exchange_rate"]),
                BaseDebitAmount = Convert.ToDecimal(row["base_debit_amount"]),
                BaseCreditAmount = Convert.ToDecimal(row["base_credit_amount"]),
                CostCenter = row["cost_center"] != DBNull.Value ? row["cost_center"].ToString() : null,
                Project = row["project"] != DBNull.Value ? row["project"].ToString() : null,
                ReconciliationStatus = row["reconciliation_status"].ToString(),
                ReconciledAt = row["reconciled_at"] != DBNull.Value ? Convert.ToDateTime(row["reconciled_at"]) : null,
                ReconciledBy = row["reconciled_by"] != DBNull.Value ? (Guid?)row["reconciled_by"] : null,
                IsOpeningBalance = Convert.ToBoolean(row["is_opening_balance"]),
                Narration = row["narration"] != DBNull.Value ? row["narration"].ToString() : null,
                CreatedAt = Convert.ToDateTime(row["created_at"])
            };
        }

        private IEnumerable<LedgerEntry> MapDataTableToLedgerEntries(DataTable dataTable)
        {
            var entries = new List<LedgerEntry>();
            foreach (DataRow row in dataTable.Rows)
            {
                entries.Add(MapDataRowToLedgerEntry(row));
            }
            return entries;
        }
    }
}
