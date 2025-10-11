using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Entities.Accounts;
using Infrastructure;
using Microsoft.Data.SqlClient;

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
            var query = @"INSERT INTO Acct.ChartOfAccounts
                        (AccountID, BusinessID, AccountTypeID, ParentAccountID, AccountCode, AccountName, Description, IsActive, IsSystemAccount, CreatedAtUTC)
                        VALUES (@AccountID, @BusinessID, @AccountTypeID, @ParentAccountID, @AccountCode, @AccountName, @Description, @IsActive, @IsSystemAccount, @CreatedAtUTC)";
            var parameters = new[]
            {
                new SqlParameter("@AccountID", chartOfAccount.AccountID),
                new SqlParameter("@BusinessID", chartOfAccount.BusinessID),
                new SqlParameter("@AccountTypeID", chartOfAccount.AccountTypeID),
                new SqlParameter("@ParentAccountID", (object)chartOfAccount.ParentAccountID ?? DBNull.Value),
                new SqlParameter("@AccountCode", chartOfAccount.AccountCode),
                new SqlParameter("@AccountName", chartOfAccount.AccountName),
                new SqlParameter("@Description", (object)chartOfAccount.Description ?? DBNull.Value),
                new SqlParameter("@IsActive", chartOfAccount.IsActive),
                new SqlParameter("@IsSystemAccount", chartOfAccount.IsSystemAccount),
                new SqlParameter("@CreatedAtUTC", chartOfAccount.CreatedAtUTC)
            };
            await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task DeleteChartOfAccountAsync(Guid id)
        {
            var query = "DELETE FROM Acct.ChartOfAccounts WHERE AccountID = @AccountID";
            var parameters = new[] { new SqlParameter("@AccountID", id) };
            await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<IEnumerable<ChartOfAccount>> GetAllChartOfAccountsAsync(Guid businessId)
        {
            var query = "SELECT * FROM Acct.ChartOfAccounts WHERE BusinessID = @BusinessID";
            var parameters = new[] { new SqlParameter("@BusinessID", businessId) };
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
            var query = "SELECT * FROM Acct.ChartOfAccounts WHERE AccountID = @AccountID";
            var parameters = new[] { new SqlParameter("@AccountID", id) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            if (dataTable.Rows.Count == 0) return null;

            return MapDataRowToChartOfAccount(dataTable.Rows[0]);
        }

        public async Task UpdateChartOfAccountAsync(ChartOfAccount chartOfAccount)
        {
            var query = @"UPDATE Acct.ChartOfAccounts
                        SET AccountName = @AccountName, Description = @Description, IsActive = @IsActive, UpdatedAtUTC = @UpdatedAtUTC
                        WHERE AccountID = @AccountID";
            var parameters = new[]
            {
                new SqlParameter("@AccountID", chartOfAccount.AccountID),
                new SqlParameter("@AccountName", chartOfAccount.AccountName),
                new SqlParameter("@Description", (object)chartOfAccount.Description ?? DBNull.Value),
                new SqlParameter("@IsActive", chartOfAccount.IsActive),
                new SqlParameter("@UpdatedAtUTC", chartOfAccount.UpdatedAtUTC)
            };
            await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
        }

        private ChartOfAccount MapDataRowToChartOfAccount(DataRow row)
        {
            return new ChartOfAccount
            {
                AccountID = (Guid)row["AccountID"],
                BusinessID = (Guid)row["BusinessID"],
                AccountTypeID = (int)row["AccountTypeID"],
                ParentAccountID = row["ParentAccountID"] as Guid?,
                AccountCode = (string)row["AccountCode"],
                AccountName = (string)row["AccountName"],
                Description = row["Description"] as string,
                IsActive = (bool)row["IsActive"],
                IsSystemAccount = (bool)row["IsSystemAccount"],
                CreatedAtUTC = (DateTime)row["CreatedAtUTC"],
                UpdatedAtUTC = row["UpdatedAtUTC"] as DateTime?
            };
        }
    }
}
