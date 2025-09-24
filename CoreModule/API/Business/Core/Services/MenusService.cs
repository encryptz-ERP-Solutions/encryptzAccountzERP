using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using AutoMapper;
using Entities.Core;
using Repository.Core.Interface;

namespace BusinessLogic.Core.Services
{
    public class MenusService : IMenusService
    {
        private readonly IMenusRepository _menusRepository;
        private readonly IMapper _mapper;

        public MenusService(IMenusRepository menusRepository, IMapper mapper)
        {
            _menusRepository = menusRepository;
            _mapper = mapper;
        }

        public async Task<bool> AddMenusAsync(MenusDto menusDto)
        {
            // TODO: Add validations
            var entity = _mapper.Map<Menus>(menusDto);
            await _menusRepository.AddAsync(entity);
            if (entity == null)
                throw new Exception("Failed to add Menus.");
            return true;
        }

        public async Task<bool> DeleteMenusAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid ID.");
            await _menusRepository.DeleteAsync(id);
            return true;
        }

        public async Task<IEnumerable<MenusDto>> GetAllMenusAsync()
        {
            var list = await _menusRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<MenusDto>>(list);
        }

        public async Task<MenusDto?> GetMenusByIdAsync(int id)
        {
            var entity = await _menusRepository.GetByIdAsync(id);
            return _mapper.Map<MenusDto>(entity);
        }

        public async Task<bool> UpdateMenusAsync(int id, MenusDto menusDto)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid ID.");
            var entity = _mapper.Map<Menus>(menusDto);
            await _menusRepository.UpdateAsync(entity);
            return true;
        }
    }
}