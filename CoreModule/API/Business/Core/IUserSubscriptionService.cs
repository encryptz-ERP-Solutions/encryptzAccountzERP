using Business.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Core
{
    public interface IUserSubscriptionService
    {
        Task<IEnumerable<UserSubscriptionDto>> GetAllAsync();
        Task<UserSubscriptionDto> GetByIdAsync(Guid id);
        Task<UserSubscriptionDto> CreateAsync(CreateUserSubscriptionDto createUserSubscriptionDto);
        Task<UserSubscriptionDto> UpdateAsync(Guid id, UpdateUserSubscriptionDto updateUserSubscriptionDto);
        Task<bool> DeleteAsync(Guid id);
    }
}
