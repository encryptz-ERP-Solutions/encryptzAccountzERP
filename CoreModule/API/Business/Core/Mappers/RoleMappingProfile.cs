using AutoMapper;
using BusinessLogic.Core.DTOs;
using Entities.Core;

namespace BusinessLogic.Core.Mappers
{
    public class RoleMappingProfile : Profile
    {
        public RoleMappingProfile()
        {
            // From DTO to Entity
            CreateMap<RoleCreateDto, Role>();
            CreateMap<RoleUpdateDto, Role>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // From Entity to DTO
            CreateMap<Role, RoleDto>();
        }
    }
}