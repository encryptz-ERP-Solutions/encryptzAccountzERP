using Entities.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Core.Interface
{
    public interface IPermissionRepository
    {
        Task<IEnumerable<Permission>> GetAllAsync();
        Task<Permission> GetByIdAsync(int id);
        Task<Permission> AddAsync(Permission permission);
        Task<bool> UpdateAsync(Permission permission);
        Task<bool> DeleteAsync(int id);
    }
}
