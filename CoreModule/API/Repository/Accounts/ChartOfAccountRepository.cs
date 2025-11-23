using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Entities.Accounts;
using Infrastructure;
using Npgsql;

namespace Repository.Accounts
{
    public class ChartOfAccountRepository : IChartOfAccountRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;

        public ChartOfAccountRepository(CoreSQLDbHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public async Task AddChartOfAccountAsync(ChartOfAccount chartOfAccount)
        {
            var query = @"INSERT INTO acct.chart_of_accounts
                        (account_id, business_id, account_type_id, parent_account_id, account_code, account_name, description, is_active, is_system_account, created_at_utc)
                        VALUES (@AccountID, @BusinessID, @AccountTypeID, @ParentAccountID, @AccountCode, @AccountName, @Description, @IsActive, @IsSystemAccount, @CreatedAtUTC)";
            var parameters = new[]
            {
                new NpgsqlParameter("@AccountID", chartOfAccount.AccountID),
                new NpgsqlParameter("@BusinessID", chartOfAccount.BusinessID),
                new NpgsqlParameter("@AccountTypeID", chartOfAccount.AccountTypeID),
                new NpgsqlParameter("@ParentAccountID", (object)chartOfAccount.ParentAccountID ?? DBNull.Value),
                new NpgsqlParameter("@AccountCode", chartOfAccount.AccountCode),
                new NpgsqlParameter("@AccountName", chartOfAccount.AccountName),
                new NpgsqlParameter("@Description", (object)chartOfAccount.Description ?? DBNull.Value),
                new NpgsqlParameter("@IsActive", chartOfAccount.IsActive),
                new NpgsqlParameter("@IsSystemAccount", chartOfAccount.IsSystemAccount),
                new NpgsqlParameter("@CreatedAtUTC", chartOfAccount.CreatedAtUTC)
            };
            await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task DeleteChartOfAccountAsync(Guid id)
        {
            var query = "DELETE FROM acct.chart_of_accounts WHERE account_id = @AccountID";
            var parameters = new[] { new NpgsqlParameter("@AccountID", id) };
            await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<IEnumerable<ChartOfAccount>> GetAllChartOfAccountsAsync(Guid businessId)
        {
            var query = "SELECT * FROM acct.chart_of_accounts WHERE business_id = @BusinessID ORDER BY account_code";
            var parameters = new[] { new NpgsqlParameter("@BusinessID", businessId) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            var chartOfAccounts = new List<ChartOfAccount>();
            foreach (DataRow row in dataTable.Rows)
            {
                chartOfAccounts.Add(MapDataRowToChartOfAccount(row));
            }
            return chartOfAccounts;
        }

        public async Task<ChartOfAccount> GetChartOfAccountByIdAsync(Guid id)
        {
            var query = "SELECT * FROM acct.chart_of_accounts WHERE account_id = @AccountID";
            var parameters = new[] { new NpgsqlParameter("@AccountID", id) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            if (dataTable.Rows.Count == 0) return null;

            return MapDataRowToChartOfAccount(dataTable.Rows[0]);
        }

        public async Task UpdateChartOfAccountAsync(ChartOfAccount chartOfAccount)
        {
            var query = @"UPDATE acct.chart_of_accounts
                        SET account_name = @AccountName, description = @Description, is_active = @IsActive, updated_at_utc = @UpdatedAtUTC
                        WHERE account_id = @AccountID";
            var parameters = new[]
            {
                new NpgsqlParameter("@AccountID", chartOfAccount.AccountID),
                new NpgsqlParameter("@AccountName", chartOfAccount.AccountName),
                new NpgsqlParameter("@Description", (object)chartOfAccount.Description ?? DBNull.Value),
                new NpgsqlParameter("@IsActive", chartOfAccount.IsActive),
                new NpgsqlParameter("@UpdatedAtUTC", chartOfAccount.UpdatedAtUTC)
            };
            await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
        }

        private ChartOfAccount MapDataRowToChartOfAccount(DataRow row)
        {
            return new ChartOfAccount
            {
                AccountID = (Guid)row["account_id"],
                BusinessID = (Guid)row["business_id"],
                AccountTypeID = (int)row["account_type_id"],
                ParentAccountID = row["parent_account_id"] == DBNull.Value ? null : (Guid?)row["parent_account_id"],
                AccountCode = (string)row["account_code"],
                AccountName = (string)row["account_name"],
                Description = row["description"] == DBNull.Value ? null : (string)row["description"],
                IsActive = (bool)row["is_active"],
                IsSystemAccount = (bool)row["is_system_account"],
                CreatedAtUTC = (DateTime)row["created_at_utc"],
                UpdatedAtUTC = row["updated_at_utc"] == DBNull.Value ? null : (DateTime?)row["updated_at_utc"]
            };
        }
    }
}
