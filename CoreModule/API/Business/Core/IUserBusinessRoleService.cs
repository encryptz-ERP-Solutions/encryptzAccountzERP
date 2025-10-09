using Shared.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Core
{
    public interface IUserBusinessRoleService
    {
        Task<IEnumerable<UserBusinessRoleDto>> GetAllAsync();
        Task<UserBusinessRoleDto> GetByIdAsync(Guid userId, Guid businessId, int roleId);
        Task AddAsync(UserBusinessRoleDto userBusinessRoleDto);
        Task DeleteAsync(Guid userId, Guid businessId, int roleId);
    }
}
