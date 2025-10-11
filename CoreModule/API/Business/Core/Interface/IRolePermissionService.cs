using Shared.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Core.Interface
{
    public interface IRolePermissionService
    {
        Task<IEnumerable<RolePermissionDto>> GetAllAsync();
        Task<RolePermissionDto> GetByIdAsync(int roleId, int permissionId);
        Task AddAsync(RolePermissionDto rolePermissionDto);
        Task DeleteAsync(int roleId, int permissionId);
    }
}
