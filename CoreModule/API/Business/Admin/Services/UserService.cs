using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Admin.DTOs;
using BusinessLogic.Admin.Interface;
using Entities.Admin;
using Repository.Admin.Interface;

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

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> GetUserByUserHandleAsync(string userHandle)
        {
            var user = await _userRepository.GetByUserHandleAsync(userHandle);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> CreateUserAsync(UserCreateDto userCreateDto)
        {
            // In a real application, you'd add validation here (e.g., using FluentValidation)
            // to ensure the user handle and email are unique before attempting to insert.

            var user = _mapper.Map<User>(userCreateDto);

            // Hash the password before saving
            user.HashedPassword = PasswordHasher.HashPassword(userCreateDto.Password);

            var addedUser = await _userRepository.AddAsync(user);
            return _mapper.Map<UserDto>(addedUser);
        }

        public async Task<bool> UpdateUserAsync(Guid id, UserUpdateDto userUpdateDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false; // User not found
            }

            // Use AutoMapper to update the existing user entity from the DTO
            _mapper.Map(userUpdateDto, user);
            user.UpdatedAtUTC = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false; // User not found
            }

            await _userRepository.DeleteAsync(id);
            return true;
        }
    }
}