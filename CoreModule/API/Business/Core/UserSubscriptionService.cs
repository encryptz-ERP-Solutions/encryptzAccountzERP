using AutoMapper;
using Business.Core.DTOs;
using Entities.Core;
using Repository.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Core
{
    public class UserSubscriptionService : IUserSubscriptionService
    {
        private readonly IUserSubscriptionRepository _userSubscriptionRepository;
        private readonly IMapper _mapper;

        public UserSubscriptionService(IUserSubscriptionRepository userSubscriptionRepository, IMapper mapper)
        {
            _userSubscriptionRepository = userSubscriptionRepository;
            _mapper = mapper;
        }

        public async Task<UserSubscriptionDto> CreateAsync(CreateUserSubscriptionDto createUserSubscriptionDto)
        {
            var userSubscription = _mapper.Map<UserSubscription>(createUserSubscriptionDto);
            var createdUserSubscription = await _userSubscriptionRepository.CreateAsync(userSubscription);
            return _mapper.Map<UserSubscriptionDto>(createdUserSubscription);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _userSubscriptionRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<UserSubscriptionDto>> GetAllAsync()
        {
            var userSubscriptions = await _userSubscriptionRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserSubscriptionDto>>(userSubscriptions);
        }

        public async Task<UserSubscriptionDto> GetByIdAsync(Guid id)
        {
            var userSubscription = await _userSubscriptionRepository.GetByIdAsync(id);
            return _mapper.Map<UserSubscriptionDto>(userSubscription);
        }

        public async Task<UserSubscriptionDto> UpdateAsync(Guid id, UpdateUserSubscriptionDto updateUserSubscriptionDto)
        {
            var userSubscription = await _userSubscriptionRepository.GetByIdAsync(id);
            if (userSubscription == null)
            {
                return null;
            }
            _mapper.Map(updateUserSubscriptionDto, userSubscription);
            var updatedUserSubscription = await _userSubscriptionRepository.UpdateAsync(userSubscription);
            return _mapper.Map<UserSubscriptionDto>(updatedUserSubscription);
        }
    }
}
