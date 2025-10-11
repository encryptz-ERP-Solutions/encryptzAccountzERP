using BusinessLogic.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Core.Interface
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
        Task<RoleDto> GetRoleByIdAsync(int id);
        Task<RoleDto> AddRoleAsync(RoleDto roleDto);
        Task<bool> UpdateRoleAsync(int id, RoleDto roleDto);
        Task<bool> DeleteRoleAsync(int id);
    }
}
