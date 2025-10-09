using Entities.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Core
{
    public interface IRolePermissionRepository
    {
        Task<IEnumerable<RolePermission>> GetAllAsync();
        Task<RolePermission> GetByIdAsync(int roleId, int permissionId);
        Task AddAsync(RolePermission rolePermission);
        Task DeleteAsync(int roleId, int permissionId);
    }
}
