using Entities.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Core
{
    public interface ISubscriptionPlanPermissionRepository
    {
        Task<IEnumerable<SubscriptionPlanPermission>> GetAllAsync();
        Task<SubscriptionPlanPermission> GetByIdAsync(int planId, int permissionId);
        Task<SubscriptionPlanPermission> CreateAsync(SubscriptionPlanPermission subscriptionPlanPermission);
        Task<SubscriptionPlanPermission> UpdateAsync(SubscriptionPlanPermission subscriptionPlanPermission);
        Task<bool> DeleteAsync(int planId, int permissionId);
        Task<IEnumerable<SubscriptionPlanPermission>> GetByPlanIdAsync(int planId);
        Task<IEnumerable<SubscriptionPlanPermission>> GetByPermissionIdAsync(int permissionId);
    }
}
