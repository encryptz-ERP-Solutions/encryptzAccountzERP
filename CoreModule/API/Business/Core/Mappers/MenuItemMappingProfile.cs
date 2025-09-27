using AutoMapper;
using BusinessLogic.Core.DTOs;
using Entities.Core;

namespace BusinessLogic.Core.Mappers
{
    public class MenuItemMappingProfile : Profile
    {
        public MenuItemMappingProfile()
        {
            // From DTO to Entity
            CreateMap<MenuItemCreateDto, MenuItem>();
            CreateMap<MenuItemUpdateDto, MenuItem>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // From Entity to DTO
            CreateMap<MenuItem, MenuItemDto>();
        }
    }
}