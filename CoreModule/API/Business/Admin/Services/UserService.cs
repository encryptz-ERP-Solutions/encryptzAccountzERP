using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Admin.DTOs;
using BusinessLogic.Admin.Interface;
using Entities.Admin;
using Repository.Admin;
using Repository.Admin.Interface;
using AutoMapper;

namespace BusinessLogic.Admin.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserDto> AddUserAsync(UserDto userDto)
        {
            var user = _mapper.Map<User>(userDto);

            user = await _userRepository.AddAsync(user);
            if (user == null)
                throw new Exception("Failed to add user.");
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> GetUserByLoginAsync(string loginValue, string loginType)
        {
            if (string.IsNullOrEmpty(loginValue) || string.IsNullOrEmpty(loginType))
                throw new ArgumentException("Login value and type are required.");

            var user = await _userRepository.GetByLoginAsync(loginValue, loginType);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> DeleteUserAsync(long id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid User ID.");

            await _userRepository.DeleteAsync(id);
            return true;
        }

        public async Task<IEnumerable<UserDto>> GetAllUserAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto?> GetUserByIdAsync(long id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> UpdateUserAsync(long id, UserDto userDto)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid User ID.");
            var userObj = _mapper.Map<User>(userDto);
            await _userRepository.UpdateAsync(userObj);
            return true;
        }
    }
}
