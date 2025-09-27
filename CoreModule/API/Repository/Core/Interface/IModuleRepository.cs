using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Core;

namespace Repository.Core.Interface
{
    public interface IModuleRepository
    {
        Task<IEnumerable<Module>> GetAllAsync();
        Task<Module?> GetByIdAsync(int id);
        Task<Module> AddAsync(Module module);
        Task<Module> UpdateAsync(Module module);
        Task<bool> DeleteAsync(int id);
    }
}