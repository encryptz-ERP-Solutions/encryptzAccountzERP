using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Core;

namespace Repository.Core.Interface
{
    public interface IUserBusinessRepository
    {
        Task<IEnumerable<UserBusiness>> GetByUserIdAsync(Guid userId);
        Task<UserBusiness> CreateAsync(Guid userId, Guid businessId, bool isDefault, Guid? createdByUserId);
        Task<bool> SetDefaultAsync(Guid userBusinessId, Guid userId, Guid? updatedByUserId);
        Task<bool> DeleteAsync(Guid userBusinessId);
        Task<Guid?> GetDefaultBusinessIdAsync(Guid userId);
    }
}

