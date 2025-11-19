using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Entities.Accounts;
using Infrastructure;
using Npgsql;

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
            var query = "INSERT INTO acct.account_types (account_type_name, normal_balance) VALUES (@AccountTypeName, @NormalBalance)";
            var parameters = new[]
            {
                new NpgsqlParameter("@AccountTypeName", accountType.AccountTypeName),
                new NpgsqlParameter("@NormalBalance", accountType.NormalBalance)
            };
            await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task DeleteAccountTypeAsync(int id)
        {
            var query = "DELETE FROM acct.account_types WHERE account_type_id = @AccountTypeID";
            var parameters = new[] { new NpgsqlParameter("@AccountTypeID", id) };
            await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<IEnumerable<AccountType>> GetAllAccountTypesAsync()
        {
            var query = "SELECT account_type_id, account_type_name, normal_balance FROM acct.account_types ORDER BY account_type_name";
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query);
            var accountTypes = new List<AccountType>();
            foreach (DataRow row in dataTable.Rows)
            {
                accountTypes.Add(new AccountType
                {
                    AccountTypeID = (int)row["account_type_id"],
                    AccountTypeName = (string)row["account_type_name"],
                    NormalBalance = (string)row["normal_balance"]
                });
            }
            return accountTypes;
        }

        public async Task<AccountType> GetAccountTypeByIdAsync(int id)
        {
            var query = "SELECT account_type_id, account_type_name, normal_balance FROM acct.account_types WHERE account_type_id = @AccountTypeID";
            var parameters = new[] { new NpgsqlParameter("@AccountTypeID", id) };
            var dataTable = await _sqlHelper.ExecuteQueryAsync(query, parameters);
            if (dataTable.Rows.Count == 0) return null;

            var row = dataTable.Rows[0];
            return new AccountType
            {
                AccountTypeID = (int)row["account_type_id"],
                AccountTypeName = (string)row["account_type_name"],
                NormalBalance = (string)row["normal_balance"]
            };
        }

        public async Task UpdateAccountTypeAsync(AccountType accountType)
        {
            var query = "UPDATE acct.account_types SET account_type_name = @AccountTypeName, normal_balance = @NormalBalance WHERE account_type_id = @AccountTypeID";
            var parameters = new[]
            {
                new NpgsqlParameter("@AccountTypeID", accountType.AccountTypeID),
                new NpgsqlParameter("@AccountTypeName", accountType.AccountTypeName),
                new NpgsqlParameter("@NormalBalance", accountType.NormalBalance)
            };
            await _sqlHelper.ExecuteNonQueryAsync(query, parameters);
        }
    }
}
