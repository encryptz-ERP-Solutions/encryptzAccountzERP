using Entities.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Core
{
    public interface IUserSubscriptionRepository
    {
        Task<IEnumerable<UserSubscription>> GetAllAsync();
        Task<UserSubscription> GetByIdAsync(Guid id);
        Task<UserSubscription> CreateAsync(UserSubscription userSubscription);
        Task<UserSubscription> UpdateAsync(UserSubscription userSubscription);
        Task<bool> DeleteAsync(Guid id);
    }
}
