using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using Entities.Core;

namespace Repository.Core.Interface
{
    public interface IMenusRepository
    {
        Task<IEnumerable<Menus>> GetAllAsync();
        Task<Menus> GetByIdAsync(int id);
        Task AddAsync(Menus module);
        Task UpdateAsync(Menus module);
        Task DeleteAsync(int id);
    }
    
}
