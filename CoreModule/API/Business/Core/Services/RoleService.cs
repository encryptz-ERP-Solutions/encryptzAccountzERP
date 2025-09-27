using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Entities.Core;
using Repository.Admin.Interface;
using Repository.Core.Interface;

namespace BusinessLogic.Core.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBusinessRepository _businessRepository;
        private readonly IMapper _mapper;

        public RoleService(
            IRoleRepository roleRepository,
            IUserRepository userRepository,
            IBusinessRepository businessRepository,
            IMapper mapper)
        {
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _businessRepository = businessRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            var roleDtos = new List<RoleDto>();

            foreach (var role in roles)
            {
                var roleDto = _mapper.Map<RoleDto>(role);
                var permissions = await _roleRepository.GetPermissionsByRoleIdAsync(role.RoleID);
                roleDto.Permissions = _mapper.Map<List<PermissionDto>>(permissions);
                roleDtos.Add(roleDto);
            }
            return roleDtos;
        }

        public async Task<RoleDto?> GetRoleByIdAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null) return null;

            var roleDto = _mapper.Map<RoleDto>(role);
            var permissions = await _roleRepository.GetPermissionsByRoleIdAsync(role.RoleID);
            roleDto.Permissions = _mapper.Map<List<PermissionDto>>(permissions);

            return roleDto;
        }

        public async Task<RoleDto> CreateRoleAsync(RoleCreateDto roleCreateDto)
        {
            var role = _mapper.Map<Role>(roleCreateDto);
            role.IsSystemRole = false; // System roles should not be created via API

            var addedRole = await _roleRepository.AddAsync(role, roleCreateDto.PermissionIDs);

            var roleDto = _mapper.Map<RoleDto>(addedRole);
            var permissions = await _roleRepository.GetPermissionsByRoleIdAsync(addedRole.RoleID);
            roleDto.Permissions = _mapper.Map<List<PermissionDto>>(permissions);

            return roleDto;
        }

        public async Task<bool> UpdateRoleAsync(int id, RoleUpdateDto roleUpdateDto)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
            {
                return false; // Role not found
            }

            // Map updatable properties from DTO to entity
            _mapper.Map(roleUpdateDto, role);

            await _roleRepository.UpdateAsync(role, roleUpdateDto.PermissionIDs ?? new List<int>());
            return true;
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null || role.IsSystemRole)
            {
                // Prevent deletion of system roles
                return false;
            }
            return await _roleRepository.DeleteAsync(id);
        }

        public async Task<bool> AssignRoleToUserAsync(AssignRoleDto assignRoleDto)
        {
            // Validate that all entities exist before making the assignment
            var user = await _userRepository.GetByIdAsync(assignRoleDto.UserID);
            if (user == null) return false;

            var business = await _businessRepository.GetByIdAsync(assignRoleDto.BusinessID);
            if (business == null) return false;

            var role = await _roleRepository.GetByIdAsync(assignRoleDto.RoleID);
            if (role == null) return false;

            await _roleRepository.AssignRoleToUserAsync(assignRoleDto.UserID, assignRoleDto.BusinessID, assignRoleDto.RoleID);
            return true;
        }
    }
}