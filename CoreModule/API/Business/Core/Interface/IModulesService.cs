using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Core.DTOs;
using Entities.Core;

namespace BusinessLogic.Core.Interface
{
    public interface IModulesService
    {
        Task<IEnumerable<ModulesDto?>> GetAllModulesAsync();
        Task<ModulesDto?> GetModulesByIdAsync(int id);
        Task<bool> AddModulesAsync(ModulesDto Modules);
        Task<bool> UpdateModulesAsync(int id, ModulesDto Modules);
        Task<bool> DeleteModulesAsync(int id);
    }
}
