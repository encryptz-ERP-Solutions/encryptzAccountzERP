using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using Entities.Core;

namespace Repository.Core.Interface
{
    public interface IModulesRepository
    {
        Task<IEnumerable<Modules>> GetAllAsync();
        Task<Modules> GetByIdAsync(int id);
        Task AddAsync(Modules module);
        Task UpdateAsync(Modules module);
        Task DeleteAsync(int id);
    }
    
}
