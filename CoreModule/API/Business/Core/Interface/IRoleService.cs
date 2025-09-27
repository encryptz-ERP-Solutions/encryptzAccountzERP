using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.Core.DTOs;

namespace BusinessLogic.Core.Interface
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
        Task<RoleDto?> GetRoleByIdAsync(int id);
        Task<RoleDto> CreateRoleAsync(RoleCreateDto roleCreateDto);
        Task<bool> UpdateRoleAsync(int id, RoleUpdateDto roleUpdateDto);
        Task<bool> DeleteRoleAsync(int id);
        Task<bool> AssignRoleToUserAsync(AssignRoleDto assignRoleDto);
    }
}