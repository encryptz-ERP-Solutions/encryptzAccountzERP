using AutoMapper;
using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Entities.Core;
using Repository.Core.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Core.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IMapper _mapper;

        public PermissionService(IPermissionRepository permissionRepository, IMapper mapper)
        {
            _permissionRepository = permissionRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
        {
            var permissions = await _permissionRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<PermissionDto>>(permissions);
        }

        public async Task<PermissionDto> GetPermissionByIdAsync(int id)
        {
            var permission = await _permissionRepository.GetByIdAsync(id);
            return _mapper.Map<PermissionDto>(permission);
        }

        public async Task<PermissionDto> AddPermissionAsync(PermissionDto permissionDto)
        {
            var permission = _mapper.Map<Permission>(permissionDto);
            var newPermission = await _permissionRepository.AddAsync(permission);
            return _mapper.Map<PermissionDto>(newPermission);
        }

        public async Task<bool> UpdatePermissionAsync(int id, PermissionDto permissionDto)
        {
            var permission = await _permissionRepository.GetByIdAsync(id);
            if (permission == null)
            {
                return false;
            }

            _mapper.Map(permissionDto, permission);
            permission.PermissionID = id; // Ensure the ID is not changed
            return await _permissionRepository.UpdateAsync(permission);
        }

        public async Task<bool> DeletePermissionAsync(int id)
        {
            return await _permissionRepository.DeleteAsync(id);
        }
    }
}
