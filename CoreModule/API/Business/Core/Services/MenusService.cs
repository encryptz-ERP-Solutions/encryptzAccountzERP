using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Entities.Core;
using Repository.Core.Interface;
using Infrastructure;
using Infrastructure.Extensions;

namespace BusinessLogic.Core.Services
{
    public class MenusService : IMenusService
    {
        private readonly IMenusRepository _MenusRepository;

        public MenusService(IMenusRepository MenusRepository)
        {
            _MenusRepository = MenusRepository;
        }

        public async Task<bool> AddMenusAsync(MenusDto MenusDto)
        {
            try
            {
                // TODO: Add validations
                var entity = MenusDto.ConvertToClassObject<MenusDto, Menus>();
                await _MenusRepository.AddAsync(entity);
                if (entity == null)
                    throw new Exception("Failed to add Menus.");
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> DeleteMenusAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Invalid ID.");
                await _MenusRepository.DeleteAsync(id);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<MenusDto>> GetAllMenusAsync()
        {
            try
            {
                var list = await _MenusRepository.GetAllAsync();
                var dtoList = new List<MenusDto>();
                foreach (var item in list)
                {
                    dtoList.Add(item.ConvertToClassObject<Menus, MenusDto>());
                }
                return dtoList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<MenusDto?> GetMenusByIdAsync(int id)
        {
            try
            {
                var entity = await _MenusRepository.GetByIdAsync(id);
                return entity?.ConvertToClassObject<Menus, MenusDto>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdateMenusAsync(int id, MenusDto MenusDto)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Invalid ID.");
                var entity = MenusDto.ConvertToClassObject<MenusDto, Menus>();
                await _MenusRepository.UpdateAsync(entity);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}