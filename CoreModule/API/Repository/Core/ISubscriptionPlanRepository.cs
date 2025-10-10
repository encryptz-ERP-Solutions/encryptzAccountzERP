using Entities.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Core
{
    public interface ISubscriptionPlanRepository
    {
        Task<IEnumerable<SubscriptionPlan>> GetAllAsync();
        Task<SubscriptionPlan> GetByIdAsync(int id);
        Task<SubscriptionPlan> CreateAsync(SubscriptionPlan subscriptionPlan);
        Task<SubscriptionPlan> UpdateAsync(SubscriptionPlan subscriptionPlan);
        Task<bool> DeleteAsync(int id);
    }
}
