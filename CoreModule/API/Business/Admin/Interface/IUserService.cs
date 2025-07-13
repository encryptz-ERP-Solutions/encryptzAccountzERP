using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Admin.DTOs;

namespace BusinessLogic.Admin.Interface
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUserAsync();
        Task<UserDto?> GetUserByIdAsync(long id);
        Task<UserDto> AddUserAsync(UserDto user);
        Task<bool> UpdateUserAsync(long id, UserDto user);
        Task<bool> DeleteUserAsync(long id);
        Task<UserDto?> GetUserByLoginAsync(string loginValue, string loginType);
    }
}
