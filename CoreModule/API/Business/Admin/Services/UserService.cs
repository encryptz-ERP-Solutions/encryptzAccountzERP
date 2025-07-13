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
using Infrastructure;
using Infrastructure.Extensions;

namespace BusinessLogic.Admin.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository) {
        _userRepository = userRepository;
        }

        public async Task<UserDto> AddUserAsync(UserDto userDto)
        {
            try
            {
                if (string.IsNullOrEmpty(userDto.userId))
                    throw new ArgumentException("userID is required.");

                if (userDto.userId.Length > 50)
                    throw new ArgumentException("userId cannot exceed 50 characters.");

                var user = userDto.ConvertToClassObject<UserDto,User>(); 

               user= await _userRepository.AddAsync(user);
                if (user == null)
                    throw new Exception("Failed to add user.");
                return user.ConvertToClassObject<User,UserDto>();
            }
            catch (Exception)
            {
                throw;
            }
           
        }

        public async Task<UserDto?> GetUserByLoginAsync(string loginValue, string loginType)
        {
            try
            {
                if (string.IsNullOrEmpty(loginValue) || string.IsNullOrEmpty(loginType))
                    throw new ArgumentException("Login value and type are required.");

                var user = await _userRepository.GetByLoginAsync(loginValue, loginType);
                return user?.ConvertToClassObject<User, UserDto>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(long id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Invalid User ID.");

                await _userRepository.DeleteAsync(id);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<UserDto>> GetAllUserAsync()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                var userDtos = new List<UserDto>();

                foreach (var user in users)
                {
                    userDtos.Add(user.ConvertToClassObject<User,UserDto>());
                }

                return userDtos;
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        public async Task<UserDto?> GetUserByIdAsync(long id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                return user?.ConvertToClassObject<User, UserDto>();
            }
            catch (Exception)
            {

                throw;
            }
           
        }

        public async Task<bool> UpdateUserAsync(long id, UserDto user)
        {
            try
            {
                if (id <=0)
                    throw new ArgumentException("Invalid User ID.");
                var userObj=user.ConvertToClassObject<UserDto,User>();
                await _userRepository.UpdateAsync(userObj);
                return true;
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        private UserDto MapToDto(User entity)
        {
            return new UserDto
            {
                userId = entity.userId,
                 userName = entity.userName,
                 userPassword = entity.userPassword,
                 panNo = entity.panNo,
                 adharCardNo = entity.adharCardNo,
                 phoneNo = entity.phoneNo,
                 address = entity.address,
                 stateId = entity.stateId,
                 nationId = entity.nationId,
                 isActive = entity.isActive

            };
        }
    }
}
