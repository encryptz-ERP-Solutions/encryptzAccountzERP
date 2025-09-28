using AutoMapper;
using BusinessLogic.Admin.DTOs;
using Entities.Admin;
using System;

namespace BusinessLogic.Admin.Mappers
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            // From DTO to Entity
            CreateMap<UserCreateDto, User>()
                .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.HashedPassword, opt => opt.Ignore()) // Password will be hashed in the service
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CreatedAtUTC, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAtUTC, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UserUpdateDto, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // From Entity to DTO
            CreateMap<User, UserDto>();
        }
    }
}