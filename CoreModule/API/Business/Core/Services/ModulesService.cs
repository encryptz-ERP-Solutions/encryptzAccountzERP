using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Core.Interface;
using Repository.Core.Interface;
using BusinessLogic.Core.DTOs;
using Entities.Core;

namespace BusinessLogic.Core.Services
{
    public class ModulesService : IModulesService
    {
        private readonly IModulesRepository _modulesRepository;
        private readonly IMapper _mapper;

        public ModulesService(IModulesRepository modulesRepository, IMapper mapper)
        {
            _modulesRepository = modulesRepository;
            _mapper = mapper;
        }

        public async Task<bool> AddModulesAsync(ModulesDto modulesDto)
        {
            // TODO: Add validations
            var entity = _mapper.Map<Modules>(modulesDto);
            await _modulesRepository.AddAsync(entity);
            if (entity == null)
                throw new Exception("Failed to add Modules.");
            return true;
        }

        public async Task<bool> DeleteModulesAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid ID.");
            await _modulesRepository.DeleteAsync(id);
            return true;
        }

        public async Task<IEnumerable<ModulesDto>> GetAllModulesAsync()
        {
            var list = await _modulesRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ModulesDto>>(list);
        }

        public async Task<ModulesDto?> GetModulesByIdAsync(int id)
        {
            var entity = await _modulesRepository.GetByIdAsync(id);
            return _mapper.Map<ModulesDto>(entity);
        }

        public async Task<bool> UpdateModulesAsync(int id, ModulesDto modulesDto)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid ID.");
            var entity = _mapper.Map<Modules>(modulesDto);
            await _modulesRepository.UpdateAsync(entity);
            return true;
        }
    }
}