using AutoMapper;
using BusinessLogic.Core.DTOs;
using Entities.Core;

namespace BusinessLogic.Core.Mappers
{
    public class BusinessMappingProfile : Profile
    {
        public BusinessMappingProfile()
        {
            CreateMap<Entities.Core.Business, BusinessDto>();
            CreateMap<BusinessDto, Entities.Core.Business>();
        }
    }
}