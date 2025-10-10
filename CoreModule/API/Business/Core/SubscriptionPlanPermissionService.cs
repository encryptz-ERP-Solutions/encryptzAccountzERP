using AutoMapper;
using Business.Core.DTOs;
using Entities.Core;
using Repository.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Core
{
    public class SubscriptionPlanPermissionService : ISubscriptionPlanPermissionService
    {
        private readonly ISubscriptionPlanPermissionRepository _subscriptionPlanPermissionRepository;
        private readonly IMapper _mapper;

        public SubscriptionPlanPermissionService(ISubscriptionPlanPermissionRepository subscriptionPlanPermissionRepository, IMapper mapper)
        {
            _subscriptionPlanPermissionRepository = subscriptionPlanPermissionRepository;
            _mapper = mapper;
        }

        public async Task<SubscriptionPlanPermissionDto> CreateAsync(CreateSubscriptionPlanPermissionDto createSubscriptionPlanPermissionDto)
        {
            var subscriptionPlanPermission = _mapper.Map<SubscriptionPlanPermission>(createSubscriptionPlanPermissionDto);
            var createdSubscriptionPlanPermission = await _subscriptionPlanPermissionRepository.CreateAsync(subscriptionPlanPermission);
            return _mapper.Map<SubscriptionPlanPermissionDto>(createdSubscriptionPlanPermission);
        }

        public async Task<bool> DeleteAsync(int planId, int permissionId)
        {
            return await _subscriptionPlanPermissionRepository.DeleteAsync(planId, permissionId);
        }

        public async Task<IEnumerable<SubscriptionPlanPermissionDto>> GetAllAsync()
        {
            var subscriptionPlanPermissions = await _subscriptionPlanPermissionRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<SubscriptionPlanPermissionDto>>(subscriptionPlanPermissions);
        }

        public async Task<SubscriptionPlanPermissionDto> GetByIdAsync(int planId, int permissionId)
        {
            var subscriptionPlanPermission = await _subscriptionPlanPermissionRepository.GetByIdAsync(planId, permissionId);
            return _mapper.Map<SubscriptionPlanPermissionDto>(subscriptionPlanPermission);
        }

        public async Task<IEnumerable<SubscriptionPlanPermissionDto>> GetByPlanIdAsync(int planId)
        {
            var subscriptionPlanPermissions = await _subscriptionPlanPermissionRepository.GetByPlanIdAsync(planId);
            return _mapper.Map<IEnumerable<SubscriptionPlanPermissionDto>>(subscriptionPlanPermissions);
        }

        public async Task<IEnumerable<SubscriptionPlanPermissionDto>> GetByPermissionIdAsync(int permissionId)
        {
            var subscriptionPlanPermissions = await _subscriptionPlanPermissionRepository.GetByPermissionIdAsync(permissionId);
            return _mapper.Map<IEnumerable<SubscriptionPlanPermissionDto>>(subscriptionPlanPermissions);
        }

        public async Task<SubscriptionPlanPermissionDto> UpdateAsync(int planId, int permissionId, UpdateSubscriptionPlanPermissionDto updateSubscriptionPlanPermissionDto)
        {
            var subscriptionPlanPermission = await _subscriptionPlanPermissionRepository.GetByIdAsync(planId, permissionId);
            if (subscriptionPlanPermission == null)
            {
                return null;
            }
            _mapper.Map(updateSubscriptionPlanPermissionDto, subscriptionPlanPermission);
            var updatedSubscriptionPlanPermission = await _subscriptionPlanPermissionRepository.UpdateAsync(subscriptionPlanPermission);
            return _mapper.Map<SubscriptionPlanPermissionDto>(updatedSubscriptionPlanPermission);
        }
    }
}
