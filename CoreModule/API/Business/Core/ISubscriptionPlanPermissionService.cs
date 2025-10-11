using Business.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Core
{
    public interface ISubscriptionPlanPermissionService
    {
        Task<IEnumerable<SubscriptionPlanPermissionDto>> GetAllAsync();
        Task<SubscriptionPlanPermissionDto> GetByIdAsync(int planId, int permissionId);
        Task<SubscriptionPlanPermissionDto> CreateAsync(CreateSubscriptionPlanPermissionDto createSubscriptionPlanPermissionDto);
        Task<SubscriptionPlanPermissionDto> UpdateAsync(int planId, int permissionId, UpdateSubscriptionPlanPermissionDto updateSubscriptionPlanPermissionDto);
        Task<bool> DeleteAsync(int planId, int permissionId);
        Task<IEnumerable<SubscriptionPlanPermissionDto>> GetByPlanIdAsync(int planId);
        Task<IEnumerable<SubscriptionPlanPermissionDto>> GetByPermissionIdAsync(int permissionId);
    }
}
