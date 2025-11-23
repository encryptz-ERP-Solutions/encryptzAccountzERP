using Entities.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Core.Interface
{
    public interface IRoleRepository
    {
        Task<Dictionary<Guid, IEnumerable<string>>> GetUserPermissionsAcrossBusinessesAsync(Guid userId);
        Task<IEnumerable<Role>> GetAllAsync();
        Task<Role> GetByIdAsync(int id);
        Task<Role> AddAsync(Role role);
        Task<bool> UpdateAsync(Role role);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(int roleId);
        Task<bool> AddPermissionToRoleAsync(int roleId, int permissionId);
        Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId);
        Task<int?> GetRoleIdByNameAsync(string roleName);
        Task<bool> UserHasAnyRoleAsync(Guid userId, IEnumerable<string> roleNames);
    }
}
