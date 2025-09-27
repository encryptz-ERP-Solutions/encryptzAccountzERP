using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.Core.DTOs;

namespace BusinessLogic.Core.Interface
{
    public interface IMenuItemService
    {
        Task<IEnumerable<MenuItemDto>> GetAllMenuItemsAsync();
        Task<MenuItemDto?> GetMenuItemByIdAsync(int id);
        Task<MenuItemDto> CreateMenuItemAsync(MenuItemCreateDto menuItemCreateDto);
        Task<bool> UpdateMenuItemAsync(int id, MenuItemUpdateDto menuItemUpdateDto);
        Task<bool> DeleteMenuItemAsync(int id);
    }
}