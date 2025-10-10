using AutoMapper;
using BusinessLogic.Accounts.DTOs;
using Entities.Accounts;

namespace BusinessLogic.Accounts.Mappers
{
    public class AccountMappingProfile : Profile
    {
        public AccountMappingProfile()
        {
            // AccountType Mappings
            CreateMap<AccountType, AccountTypeDto>();
            CreateMap<CreateAccountTypeDto, AccountType>();
            CreateMap<UpdateAccountTypeDto, AccountType>();

            // ChartOfAccount Mappings
            CreateMap<ChartOfAccount, ChartOfAccountDto>();
            CreateMap<CreateChartOfAccountDto, ChartOfAccount>();
            CreateMap<UpdateChartOfAccountDto, ChartOfAccount>();
        }
    }
}
