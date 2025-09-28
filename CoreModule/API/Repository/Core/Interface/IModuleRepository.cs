using Entities.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Core.Interface
{
    public interface IModuleRepository
    {
        Task<IEnumerable<Module>> GetAllAsync();
        Task<Module> GetByIdAsync(int id);
        Task<Module> AddAsync(Module module);
        Task<bool> UpdateAsync(Module module);
        Task<bool> DeleteAsync(int id);
    }
}