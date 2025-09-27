using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Entities.Core;
using Repository.Core.Interface;

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

        public async Task<ModuleDto?> GetModuleByIdAsync(int id)
        {
            var module = await _moduleRepository.GetByIdAsync(id);
            return _mapper.Map<ModuleDto>(module);
        }

        public async Task<ModuleDto> CreateModuleAsync(ModuleCreateDto moduleCreateDto)
        {
            var module = _mapper.Map<Module>(moduleCreateDto);
            module.IsActive = true;
            module.IsSystemModule = false; // System modules should be seeded, not created via API

            var addedModule = await _moduleRepository.AddAsync(module);
            return _mapper.Map<ModuleDto>(addedModule);
        }

        public async Task<bool> UpdateModuleAsync(int id, ModuleUpdateDto moduleUpdateDto)
        {
            var module = await _moduleRepository.GetByIdAsync(id);
            if (module == null)
            {
                return false; // Module not found
            }

            _mapper.Map(moduleUpdateDto, module);
            await _moduleRepository.UpdateAsync(module);
            return true;
        }

        public async Task<bool> DeleteModuleAsync(int id)
        {
            var module = await _moduleRepository.GetByIdAsync(id);
            if (module == null || module.IsSystemModule)
            {
                // Prevent deletion of system modules
                return false;
            }
            return await _moduleRepository.DeleteAsync(id);
        }
    }
}