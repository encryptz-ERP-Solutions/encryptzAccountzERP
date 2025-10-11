using Business.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Core
{
    public interface ISubscriptionPlanService
    {
        Task<IEnumerable<SubscriptionPlanDto>> GetAllAsync();
        Task<SubscriptionPlanDto> GetByIdAsync(int id);
        Task<SubscriptionPlanDto> CreateAsync(CreateSubscriptionPlanDto createSubscriptionPlanDto);
        Task<SubscriptionPlanDto> UpdateAsync(int id, UpdateSubscriptionPlanDto updateSubscriptionPlanDto);
        Task<bool> DeleteAsync(int id);
    }
}
