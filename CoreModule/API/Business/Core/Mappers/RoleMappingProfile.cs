using AutoMapper;
using BusinessLogic.Core.DTOs;
using Entities.Core;

namespace BusinessLogic.Core.Mappers
{
    public class RoleMappingProfile : Profile
    {
        public RoleMappingProfile()
        {
            // From Entity to DTO
            CreateMap<Role, RoleDto>()
                .ForMember(dest => dest.CreatedByUserID, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAtUTC, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedByUserID, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAtUTC, opt => opt.Ignore());

            // From DTO to Entity
            CreateMap<RoleDto, Role>()
                .ForMember(dest => dest.UserBusinessRoles, opt => opt.Ignore())
                .ForMember(dest => dest.RolePermissions, opt => opt.Ignore());
        }
    }
}

