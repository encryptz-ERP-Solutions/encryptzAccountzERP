using AutoMapper;
using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Entities.Core;
using Repository.Core.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Core.Services
{
    public class ModuleService : IModuleService
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly IMapper _mapper;

        public ModuleService(IModuleRepository moduleRepository, IMapper mapper)
        {
            _moduleRepository = moduleRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ModuleDto>> GetAllModulesAsync()
        {
            var modules = await _moduleRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ModuleDto>>(modules);
        }

        public async Task<ModuleDto> GetModuleByIdAsync(int id)
        {
            var module = await _moduleRepository.GetByIdAsync(id);
            return _mapper.Map<ModuleDto>(module);
        }

        public async Task<ModuleDto> AddModuleAsync(ModuleDto moduleDto)
        {
            var module = _mapper.Map<Module>(moduleDto);
            var newModule = await _moduleRepository.AddAsync(module);
            return _mapper.Map<ModuleDto>(newModule);
        }

        public async Task<bool> UpdateModuleAsync(int id, ModuleDto moduleDto)
        {
            var module = await _moduleRepository.GetByIdAsync(id);
            if (module == null)
            {
                return false;
            }

            _mapper.Map(moduleDto, module);
            module.ModuleID = id; // Ensure the ID is not changed
            return await _moduleRepository.UpdateAsync(module);
        }

        public async Task<bool> DeleteModuleAsync(int id)
        {
            return await _moduleRepository.DeleteAsync(id);
        }
    }
}