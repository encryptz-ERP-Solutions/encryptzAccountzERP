using AutoMapper;
using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Entities.Core;
using Repository.Core.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Core.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;

        public RoleService(IRoleRepository roleRepository, IMapper mapper)
        {
            _roleRepository = roleRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<RoleDto>>(roles);
        }

        public async Task<RoleDto> GetRoleByIdAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            return _mapper.Map<RoleDto>(role);
        }

        public async Task<RoleDto> AddRoleAsync(RoleDto roleDto)
        {
            var role = _mapper.Map<Role>(roleDto);
            var newRole = await _roleRepository.AddAsync(role);
            return _mapper.Map<RoleDto>(newRole);
        }

        public async Task<bool> UpdateRoleAsync(int id, RoleDto roleDto)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
            {
                return false;
            }

            _mapper.Map(roleDto, role);
            role.RoleID = id; // Ensure the ID is not changed
            return await _roleRepository.UpdateAsync(role);
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            return await _roleRepository.DeleteAsync(id);
        }
    }
}
