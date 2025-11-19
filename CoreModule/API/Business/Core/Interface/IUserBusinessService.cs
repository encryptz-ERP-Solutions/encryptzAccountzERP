using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.Core.DTOs;

namespace BusinessLogic.Core.Interface
{
    public interface IUserBusinessService
    {
        Task<IEnumerable<UserBusinessDto>> GetByUserIdAsync(Guid userId);
        Task<UserBusinessDto> CreateAsync(CreateUserBusinessRequest request, Guid? createdByUserId);
        Task<bool> SetDefaultAsync(Guid userBusinessId, Guid userId, Guid? updatedByUserId);
        Task<bool> DeleteAsync(Guid userBusinessId);
        Task<Guid?> GetDefaultBusinessIdAsync(Guid userId);
    }
}

