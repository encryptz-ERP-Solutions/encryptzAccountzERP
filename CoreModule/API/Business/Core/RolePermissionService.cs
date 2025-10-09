using AutoMapper;
using Business.Core;
using Entities.Core;
using Repository.Core;
using Shared.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Core
{
    public class RolePermissionService : IRolePermissionService
    {
        private readonly IRolePermissionRepository _repository;
        private readonly IMapper _mapper;

        public RolePermissionService(IRolePermissionRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task AddAsync(RolePermissionDto rolePermissionDto)
        {
            var rolePermission = _mapper.Map<RolePermission>(rolePermissionDto);
            await _repository.AddAsync(rolePermission);
        }

        public async Task DeleteAsync(int roleId, int permissionId)
        {
            await _repository.DeleteAsync(roleId, permissionId);
        }

        public async Task<IEnumerable<RolePermissionDto>> GetAllAsync()
        {
            var rolePermissions = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<RolePermissionDto>>(rolePermissions);
        }

        public async Task<RolePermissionDto> GetByIdAsync(int roleId, int permissionId)
        {
            var rolePermission = await _repository.GetByIdAsync(roleId, permissionId);
            return _mapper.Map<RolePermissionDto>(rolePermission);
        }
    }
}
