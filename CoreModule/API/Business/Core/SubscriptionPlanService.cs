using AutoMapper;
using Business.Core.DTOs;
using Entities.Core;
using Repository.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Core
{
    public class SubscriptionPlanService : ISubscriptionPlanService
    {
        private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
        private readonly IMapper _mapper;

        public SubscriptionPlanService(ISubscriptionPlanRepository subscriptionPlanRepository, IMapper mapper)
        {
            _subscriptionPlanRepository = subscriptionPlanRepository;
            _mapper = mapper;
        }

        public async Task<SubscriptionPlanDto> CreateAsync(CreateSubscriptionPlanDto createSubscriptionPlanDto)
        {
            var subscriptionPlan = _mapper.Map<SubscriptionPlan>(createSubscriptionPlanDto);
            var createdSubscriptionPlan = await _subscriptionPlanRepository.CreateAsync(subscriptionPlan);
            return _mapper.Map<SubscriptionPlanDto>(createdSubscriptionPlan);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _subscriptionPlanRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<SubscriptionPlanDto>> GetAllAsync()
        {
            var subscriptionPlans = await _subscriptionPlanRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<SubscriptionPlanDto>>(subscriptionPlans);
        }

        public async Task<SubscriptionPlanDto> GetByIdAsync(int id)
        {
            var subscriptionPlan = await _subscriptionPlanRepository.GetByIdAsync(id);
            return _mapper.Map<SubscriptionPlanDto>(subscriptionPlan);
        }

        public async Task<SubscriptionPlanDto> UpdateAsync(int id, UpdateSubscriptionPlanDto updateSubscriptionPlanDto)
        {
            var subscriptionPlan = await _subscriptionPlanRepository.GetByIdAsync(id);
            if (subscriptionPlan == null)
            {
                return null;
            }
            _mapper.Map(updateSubscriptionPlanDto, subscriptionPlan);
            var updatedSubscriptionPlan = await _subscriptionPlanRepository.UpdateAsync(subscriptionPlan);
            return _mapper.Map<SubscriptionPlanDto>(updatedSubscriptionPlan);
        }
    }
}
