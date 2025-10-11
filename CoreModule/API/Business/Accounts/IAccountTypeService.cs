using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.Accounts.DTOs;

namespace BusinessLogic.Accounts
{
    public interface IAccountTypeService
    {
        Task<IEnumerable<AccountTypeDto>> GetAllAccountTypesAsync();
        Task<AccountTypeDto> GetAccountTypeByIdAsync(int id);
        Task<AccountTypeDto> CreateAccountTypeAsync(CreateAccountTypeDto createAccountTypeDto);
        Task UpdateAccountTypeAsync(int id, UpdateAccountTypeDto updateAccountTypeDto);
        Task DeleteAccountTypeAsync(int id);
    }
}
