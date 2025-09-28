using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.Admin.DTOs;

namespace BusinessLogic.Admin.Interface
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(Guid id);
        Task<UserDto?> GetUserByUserHandleAsync(string userHandle);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<UserDto> CreateUserAsync(UserCreateDto userCreateDto);
        Task<bool> UpdateUserAsync(Guid id, UserUpdateDto userUpdateDto);
        Task<bool> DeleteUserAsync(Guid id);
    }
}