using AutoMapper;
using BusinessLogic.Accounts.DTOs;
using Entities.Accounts;

namespace BusinessLogic.Accounts.Mappers
{
    public class AccountMappingProfile : Profile
    {
        public AccountMappingProfile()
        {
            // Transaction Mappings
            CreateMap<TransactionHeader, TransactionHeaderDto>();
            CreateMap<TransactionDetail, TransactionDetailDto>();
            CreateMap<CreateTransactionDto, TransactionHeader>();
            CreateMap<CreateTransactionDetailDto, TransactionDetail>();
            CreateMap<UpdateTransactionHeaderDto, TransactionHeader>();
        }
    }
}
