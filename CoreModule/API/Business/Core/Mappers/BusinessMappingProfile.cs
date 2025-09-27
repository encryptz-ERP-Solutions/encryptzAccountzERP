using AutoMapper;
using BusinessLogic.Core.DTOs;
using Entities.Core;

namespace BusinessLogic.Core.Mappers
{
    public class BusinessMappingProfile : Profile
    {
        public BusinessMappingProfile()
        {
            // From DTO to Entity
            CreateMap<BusinessCreateDto, Business>();
            CreateMap<BusinessUpdateDto, Business>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // From Entity to DTO
            CreateMap<Business, BusinessDto>();
        }
    }
}