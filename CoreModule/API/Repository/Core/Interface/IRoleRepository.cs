using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Core;

namespace Repository.Core.Interface
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetAllAsync();
        Task<Role?> GetByIdAsync(int id);
        Task<Role> AddAsync(Role role, IEnumerable<int> permissionIds);
        Task<Role> UpdateAsync(Role role, IEnumerable<int> permissionIds);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(int roleId);
        Task AssignRoleToUserAsync(Guid userId, Guid businessId, int roleId);
        Task<Dictionary<Guid, IEnumerable<string>>> GetUserPermissionsAcrossBusinessesAsync(Guid userId);
    }
}