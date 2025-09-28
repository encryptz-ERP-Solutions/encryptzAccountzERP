using BusinessLogic.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Core.Interface
{
    public interface IMenuItemService
    {
        Task<IEnumerable<MenuItemDto>> GetAllMenuItemsAsync();
        Task<MenuItemDto> GetMenuItemByIdAsync(int id);
        Task<MenuItemDto> AddMenuItemAsync(MenuItemDto menuItemDto);
        Task<bool> UpdateMenuItemAsync(int id, MenuItemDto menuItemDto);
        Task<bool> DeleteMenuItemAsync(int id);
    }
}