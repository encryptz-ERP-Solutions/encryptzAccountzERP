using AutoMapper;
using BusinessLogic.Core.DTOs;
using Entities.Core;

namespace BusinessLogic.Core.Mappers
{
    public class ModuleMappingProfile : Profile
    {
        public ModuleMappingProfile()
        {
            // From DTO to Entity
            CreateMap<ModuleCreateDto, Module>();
            CreateMap<ModuleUpdateDto, Module>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // From Entity to DTO
            CreateMap<Module, ModuleDto>();
        }
    }
}