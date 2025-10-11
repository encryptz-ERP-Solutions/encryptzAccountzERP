using AutoMapper;
using BusinessLogic.Core.Interface;
using Entities.Core;
using Repository.Core;
using Shared.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Core.Services
{
    public class UserBusinessRoleService : IUserBusinessRoleService
    {
        private readonly IUserBusinessRoleRepository _repository;
        private readonly IMapper _mapper;

        public UserBusinessRoleService(IUserBusinessRoleRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task AddAsync(UserBusinessRoleDto userBusinessRoleDto)
        {
            var userBusinessRole = _mapper.Map<UserBusinessRole>(userBusinessRoleDto);
            await _repository.AddAsync(userBusinessRole);
        }

        public async Task DeleteAsync(Guid userId, Guid businessId, int roleId)
        {
            await _repository.DeleteAsync(userId, businessId, roleId);
        }

        public async Task<IEnumerable<UserBusinessRoleDto>> GetAllAsync()
        {
            var userBusinessRoles = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserBusinessRoleDto>>(userBusinessRoles);
        }

        public async Task<UserBusinessRoleDto> GetByIdAsync(Guid userId, Guid businessId, int roleId)
        {
            var userBusinessRole = await _repository.GetByIdAsync(userId, businessId, roleId);
            return _mapper.Map<UserBusinessRoleDto>(userBusinessRole);
        }
    }
}
