using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Accounts;

namespace Repository.Accounts
{
    public interface IAccountTypeRepository
    {
        Task<IEnumerable<AccountType>> GetAllAccountTypesAsync();
        Task<AccountType> GetAccountTypeByIdAsync(int id);
        Task AddAccountTypeAsync(AccountType accountType);
        Task UpdateAccountTypeAsync(AccountType accountType);
        Task DeleteAccountTypeAsync(int id);
    }
}
