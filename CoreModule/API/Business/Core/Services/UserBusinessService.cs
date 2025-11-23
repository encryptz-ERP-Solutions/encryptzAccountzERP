using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Repository.Core.Interface;

namespace BusinessLogic.Core.Services
{
    public class UserBusinessService : IUserBusinessService
    {
        private readonly IUserBusinessRepository _repository;
        private readonly IMapper _mapper;

        public UserBusinessService(IUserBusinessRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserBusinessDto>> GetByUserIdAsync(Guid userId)
        {
            var entities = await _repository.GetByUserIdAsync(userId);
            return entities.Select(e => _mapper.Map<UserBusinessDto>(e));
        }

        public async Task<UserBusinessDto> CreateAsync(CreateUserBusinessRequest request, Guid? createdByUserId)
        {
            var entity = await _repository.CreateAsync(
                request.UserID, 
                request.BusinessID, 
                request.IsDefault ?? false, 
                createdByUserId);
            return _mapper.Map<UserBusinessDto>(entity);
        }

        public async Task<bool> SetDefaultAsync(Guid userBusinessId, Guid userId, Guid? updatedByUserId)
        {
            return await _repository.SetDefaultAsync(userBusinessId, userId, updatedByUserId);
        }

        public async Task<bool> DeleteAsync(Guid userBusinessId)
        {
            return await _repository.DeleteAsync(userBusinessId);
        }

        public async Task<Guid?> GetDefaultBusinessIdAsync(Guid userId)
        {
            return await _repository.GetDefaultBusinessIdAsync(userId);
        }
    }
}

