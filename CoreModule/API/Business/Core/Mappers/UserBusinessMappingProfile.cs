using AutoMapper;
using BusinessLogic.Core.DTOs;
using Entities.Core;

namespace BusinessLogic.Core.Mappers
{
    public class UserBusinessMappingProfile : Profile
    {
        public UserBusinessMappingProfile()
        {
            CreateMap<UserBusiness, UserBusinessDto>();
            CreateMap<UserBusinessDto, UserBusiness>();
        }
    }
}

