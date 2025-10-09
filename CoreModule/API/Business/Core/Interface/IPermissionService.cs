using BusinessLogic.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Core.Interface
{
    public interface IPermissionService
    {
        Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync();
        Task<PermissionDto> GetPermissionByIdAsync(int id);
        Task<PermissionDto> AddPermissionAsync(PermissionDto permissionDto);
        Task<bool> UpdatePermissionAsync(int id, PermissionDto permissionDto);
        Task<bool> DeletePermissionAsync(int id);
    }
}
