using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.Core.DTOs;

namespace BusinessLogic.Core.Interface
{
    public interface IModuleService
    {
        Task<IEnumerable<ModuleDto>> GetAllModulesAsync();
        Task<ModuleDto?> GetModuleByIdAsync(int id);
        Task<ModuleDto> CreateModuleAsync(ModuleCreateDto moduleCreateDto);
        Task<bool> UpdateModuleAsync(int id, ModuleUpdateDto moduleUpdateDto);
        Task<bool> DeleteModuleAsync(int id);
    }
}