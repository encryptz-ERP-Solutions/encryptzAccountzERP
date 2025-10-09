using Entities.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Core
{
    public interface IUserBusinessRoleRepository
    {
        Task<IEnumerable<UserBusinessRole>> GetAllAsync();
        Task<UserBusinessRole> GetByIdAsync(Guid userId, Guid businessId, int roleId);
        Task AddAsync(UserBusinessRole userBusinessRole);
        Task DeleteAsync(Guid userId, Guid businessId, int roleId);
    }
}
