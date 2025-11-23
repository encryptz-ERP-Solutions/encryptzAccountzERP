using Entities.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Core.Interface
{
    public interface IMenuItemRepository
    {
        Task<IEnumerable<MenuItem>> GetAllAsync();
        Task<IEnumerable<MenuItem>> GetByModuleIdAsync(int moduleId);
        Task<MenuItem> GetByIdAsync(int id);
        Task<MenuItem> AddAsync(MenuItem menuItem);
        Task<bool> UpdateAsync(MenuItem menuItem);
        Task<bool> DeleteAsync(int id);
    }
}