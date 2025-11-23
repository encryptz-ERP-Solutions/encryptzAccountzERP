using BusinessLogic.Core.DTOs;
using System;
using System.Threading.Tasks;

namespace BusinessLogic.Core.Interface
{
    public interface IUserProfileService
    {
        Task<UserProfileDto?> GetProfileAsync(Guid userId);
        Task<bool> UpdateProfileAsync(Guid userId, UpdateUserProfileDto profileDto);
    }
}

