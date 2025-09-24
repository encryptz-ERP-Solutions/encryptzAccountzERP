using AutoMapper;
using BusinessLogic.Admin.DTOs;
using BusinessLogic.Core.DTOs;
using Entities.Admin;
using Entities.Core;

namespace BusinessLogic.Mappers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<Company, CompanyDto>().ReverseMap();
            CreateMap<Menus, MenusDto>().ReverseMap();
            CreateMap<Modules, ModulesDto>().ReverseMap();
        }
    }
}
