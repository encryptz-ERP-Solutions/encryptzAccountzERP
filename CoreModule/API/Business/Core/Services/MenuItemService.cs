using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Entities.Core;
using Repository.Core.Interface;

namespace BusinessLogic.Core.Services
{
    public class MenuItemService : IMenuItemService
    {
        private readonly IMenuItemRepository _menuItemRepository;
        private readonly IMapper _mapper;

        public MenuItemService(IMenuItemRepository menuItemRepository, IMapper mapper)
        {
            _menuItemRepository = menuItemRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MenuItemDto>> GetAllMenuItemsAsync()
        {
            var menuItems = await _menuItemRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<MenuItemDto>>(menuItems);
        }

        public async Task<MenuItemDto?> GetMenuItemByIdAsync(int id)
        {
            var menuItem = await _menuItemRepository.GetByIdAsync(id);
            return _mapper.Map<MenuItemDto>(menuItem);
        }

        public async Task<MenuItemDto> CreateMenuItemAsync(MenuItemCreateDto menuItemCreateDto)
        {
            var menuItem = _mapper.Map<MenuItem>(menuItemCreateDto);
            menuItem.IsActive = true;

            var addedMenuItem = await _menuItemRepository.AddAsync(menuItem);
            return _mapper.Map<MenuItemDto>(addedMenuItem);
        }

        public async Task<bool> UpdateMenuItemAsync(int id, MenuItemUpdateDto menuItemUpdateDto)
        {
            var menuItem = await _menuItemRepository.GetByIdAsync(id);
            if (menuItem == null)
            {
                return false; // Menu item not found
            }

            _mapper.Map(menuItemUpdateDto, menuItem);
            await _menuItemRepository.UpdateAsync(menuItem);
            return true;
        }

        public async Task<bool> DeleteMenuItemAsync(int id)
        {
            return await _menuItemRepository.DeleteAsync(id);
        }
    }
}