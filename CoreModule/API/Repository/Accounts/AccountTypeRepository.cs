using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Entities.Accounts;
using Infrastructure;
using Microsoft.Data.SqlClient;

namespace Repository.Accounts
{
    public class AccountTypeRepository : IAccountTypeRepository
    {
        private readonly CoreSQLDbHelper _sqlHelper;

        public AccountTypeRepository(CoreSQLDbHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public async Task AddAccountTypeAsync(AccountType accountType)
        {
            var query = "INSERT INTO Acct.AccountTypes (AccountTypeName, NormalBalance) VALUES (@AccountTypeName, @NormalBalance)";
            var parameters = new[]
            {
                new SqlParameter("@AccountTypeName", accountType.AccountTypeName),
                new SqlParameter("@NormalBalance", accountType.NormalBalance)
            };
            await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task DeleteAccountTypeAsync(int id)
        {
            var query = "DELETE FROM Acct.AccountTypes WHERE AccountTypeID = @AccountTypeID";
            var parameters = new[] { new SqlParameter("@AccountTypeID", id) };
            await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<IEnumerable<AccountType>> GetAllAccountTypesAsync()
        {
            var query = "SELECT AccountTypeID, AccountTypeName, NormalBalance FROM Acct.AccountTypes";
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query);
            var accountTypes = new List<AccountType>();
            foreach (DataRow row in dataTable.Rows)
            {
                accountTypes.Add(new AccountType
                {
                    AccountTypeID = (int)row["AccountTypeID"],
                    AccountTypeName = (string)row["AccountTypeName"],
                    NormalBalance = (string)row["NormalBalance"]
                });
            }
            return accountTypes;
        }

        public async Task<AccountType> GetAccountTypeByIdAsync(int id)
        {
            var query = "SELECT AccountTypeID, AccountTypeName, NormalBalance FROM Acct.AccountTypes WHERE AccountTypeID = @AccountTypeID";
            var parameters = new[] { new SqlParameter("@AccountTypeID", id) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            if (dataTable.Rows.Count == 0) return null;

            var row = dataTable.Rows[0];
            return new AccountType
            {
                AccountTypeID = (int)row["AccountTypeID"],
                AccountTypeName = (string)row["AccountTypeName"],
                NormalBalance = (string)row["NormalBalance"]
            };
        }

        public async Task UpdateAccountTypeAsync(AccountType accountType)
        {
            var query = "UPDATE Acct.AccountTypes SET AccountTypeName = @AccountTypeName, NormalBalance = @NormalBalance WHERE AccountTypeID = @AccountTypeID";
            var parameters = new[]
            {
                new SqlParameter("@AccountTypeID", accountType.AccountTypeID),
                new SqlParameter("@AccountTypeName", accountType.AccountTypeName),
                new SqlParameter("@NormalBalance", accountType.NormalBalance)
            };
            await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
        }
    }
}
