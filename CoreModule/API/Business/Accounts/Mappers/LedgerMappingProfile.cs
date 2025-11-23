using AutoMapper;
using BusinessLogic.Accounts.DTOs;
using Entities.Accounts;

namespace BusinessLogic.Accounts.Mappers
{
    public class LedgerMappingProfile : Profile
    {
        public LedgerMappingProfile()
        {
            CreateMap<LedgerEntry, LedgerEntryDto>()
                .ForMember(dest => dest.AccountName, opt => opt.MapFrom(src => src.Account != null ? src.Account.AccountName : string.Empty));
        }
    }
}
