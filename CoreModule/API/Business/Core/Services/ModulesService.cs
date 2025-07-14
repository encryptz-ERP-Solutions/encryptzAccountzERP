using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
using BusinessLogic.Core.Interface;
using Repository.Core.Interface;
using BusinessLogic.Core.DTOs;
using Entities.Core;

namespace BusinessLogic.Core.Services
{
    public class ModulesService : IModulesService
    {
        private readonly IModulesRepository _ModulesRepository;

        public ModulesService(IModulesRepository ModulesRepository)
        {
            _ModulesRepository = ModulesRepository;
        }

        public async Task<bool> AddModulesAsync(ModulesDto ModulesDto)
        {
            try
            {
                // TODO: Add validations
                var entity = ModulesDto.ConvertToClassObject<ModulesDto, Modules>();
                 await _ModulesRepository.AddAsync(entity);
                if (entity == null)
                    throw new Exception("Failed to add Modules.");
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> DeleteModulesAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Invalid ID.");
                await _ModulesRepository.DeleteAsync(id);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<ModulesDto>> GetAllModulesAsync()
        {
            try
            {
                var list = await _ModulesRepository.GetAllAsync();
                var dtoList = new List<ModulesDto>();
                foreach (var item in list)
                {
                    dtoList.Add(item.ConvertToClassObject<Modules, ModulesDto>());
                }
                return dtoList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ModulesDto?> GetModulesByIdAsync(int id)
        {
            try
            {
                var entity = await _ModulesRepository.GetByIdAsync(id);
                return entity?.ConvertToClassObject<Modules, ModulesDto>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdateModulesAsync(int id, ModulesDto ModulesDto)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Invalid ID.");
                var entity = ModulesDto.ConvertToClassObject<ModulesDto, Modules>();
                await _ModulesRepository.UpdateAsync(entity);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}