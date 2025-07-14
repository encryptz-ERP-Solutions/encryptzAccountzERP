using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Core.DTOs;
using Entities.Core;

namespace BusinessLogic.Core.Interface
{
    public interface IMenusService
    {
        Task<IEnumerable<MenusDto?>> GetAllMenusAsync();
        Task<MenusDto?> GetMenusByIdAsync(int id);
        Task<bool> AddMenusAsync(MenusDto Menus);
        Task<bool> UpdateMenusAsync(int id, MenusDto Menus);
        Task<bool> DeleteMenusAsync(int id);
    }
}
