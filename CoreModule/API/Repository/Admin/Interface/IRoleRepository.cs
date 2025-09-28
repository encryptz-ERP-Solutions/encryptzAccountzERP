using Entities.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Admin.Interface
{
    public interface IRoleRepository
    {
        Task<Dictionary<Guid, IEnumerable<string>>> GetUserPermissionsAcrossBusinessesAsync(Guid userId);
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<Role> GetRoleByIdAsync(int roleId);
        Task<Role> AddRoleAsync(Role role);
        Task<bool> UpdateRoleAsync(Role role);
        Task<bool> DeleteRoleAsync(int roleId);
        Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(int roleId);
        Task<bool> AddPermissionToRoleAsync(int roleId, int permissionId);
        Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId);
    }
}