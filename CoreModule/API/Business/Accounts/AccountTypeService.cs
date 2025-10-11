using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Accounts.DTOs;
using Entities.Accounts;
using Repository.Accounts;

namespace BusinessLogic.Accounts
{
    public class AccountTypeService : IAccountTypeService
    {
        private readonly IAccountTypeRepository _accountTypeRepository;
        private readonly IMapper _mapper;

        public AccountTypeService(IAccountTypeRepository accountTypeRepository, IMapper mapper)
        {
            _accountTypeRepository = accountTypeRepository;
            _mapper = mapper;
        }

        public async Task<AccountTypeDto> CreateAccountTypeAsync(CreateAccountTypeDto createAccountTypeDto)
        {
            var accountType = _mapper.Map<AccountType>(createAccountTypeDto);
            await _accountTypeRepository.AddAccountTypeAsync(accountType);
            // This is a simplification. In a real app, you'd get the created entity back from the repo.
            return _mapper.Map<AccountTypeDto>(accountType);
        }

        public async Task DeleteAccountTypeAsync(int id)
        {
            await _accountTypeRepository.DeleteAccountTypeAsync(id);
        }

        public async Task<IEnumerable<AccountTypeDto>> GetAllAccountTypesAsync()
        {
            var accountTypes = await _accountTypeRepository.GetAllAccountTypesAsync();
            return _mapper.Map<IEnumerable<AccountTypeDto>>(accountTypes);
        }

        public async Task<AccountTypeDto> GetAccountTypeByIdAsync(int id)
        {
            var accountType = await _accountTypeRepository.GetAccountTypeByIdAsync(id);
            return _mapper.Map<AccountTypeDto>(accountType);
        }

        public async Task UpdateAccountTypeAsync(int id, UpdateAccountTypeDto updateAccountTypeDto)
        {
            var accountType = await _accountTypeRepository.GetAccountTypeByIdAsync(id);
            if (accountType == null)
            {
                // In a real app, you would throw a NotFoundException or similar.
                return;
            }
            _mapper.Map(updateAccountTypeDto, accountType);
            await _accountTypeRepository.UpdateAccountTypeAsync(accountType);
        }
    }
}
