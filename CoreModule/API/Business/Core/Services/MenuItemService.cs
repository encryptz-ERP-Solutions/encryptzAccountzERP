using AutoMapper;
using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Entities.Core;
using Repository.Core.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public async Task<MenuItemDto> GetMenuItemByIdAsync(int id)
        {
            var menuItem = await _menuItemRepository.GetByIdAsync(id);
            return _mapper.Map<MenuItemDto>(menuItem);
        }

        public async Task<MenuItemDto> AddMenuItemAsync(MenuItemDto menuItemDto)
        {
            var menuItem = _mapper.Map<MenuItem>(menuItemDto);
            var newMenuItem = await _menuItemRepository.AddAsync(menuItem);
            return _mapper.Map<MenuItemDto>(newMenuItem);
        }

        public async Task<bool> UpdateMenuItemAsync(int id, MenuItemDto menuItemDto)
        {
            var menuItem = await _menuItemRepository.GetByIdAsync(id);
            if (menuItem == null)
            {
                return false;
            }

            _mapper.Map(menuItemDto, menuItem);
            menuItem.MenuItemID = id; // Ensure the ID is not changed
            return await _menuItemRepository.UpdateAsync(menuItem);
        }

        public async Task<bool> DeleteMenuItemAsync(int id)
        {
            return await _menuItemRepository.DeleteAsync(id);
        }
    }
}