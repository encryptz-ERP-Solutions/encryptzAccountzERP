using BusinessLogic.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Core.Interface
{
    public interface IModuleService
    {
        Task<IEnumerable<ModuleDto>> GetAllModulesAsync();
        Task<ModuleDto> GetModuleByIdAsync(int id);
        Task<ModuleDto> AddModuleAsync(ModuleDto moduleDto);
        Task<bool> UpdateModuleAsync(int id, ModuleDto moduleDto);
        Task<bool> DeleteModuleAsync(int id);
    }
}