using AutoMapper;
using Entities.Core;
using Shared.Core;

namespace BusinessLogic.Core.Mappers
{
    public class UserBusinessRoleMappingProfile : Profile
    {
        public UserBusinessRoleMappingProfile()
        {
            CreateMap<UserBusinessRoleDto, UserBusinessRole>();
            CreateMap<UserBusinessRole, UserBusinessRoleDto>();
        }
    }
}

