using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Infrastructure.Extensions;
using Repository.Admin.Interface;
using System;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Core.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserRepository _userRepository;

        public UserProfileService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserProfileDto?> GetProfileAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var hasPan = user.PanCardNumber_Encrypted != null && user.PanCardNumber_Encrypted.Length > 0;

            return new UserProfileDto
            {
                UserId = user.UserID,
                UserHandle = user.UserHandle,
                FullName = user.FullName,
                Email = user.Email,
                MobileCountryCode = user.MobileCountryCode,
                MobileNumber = user.MobileNumber,
                HasPanCard = hasPan,
                IsProfileComplete = !string.IsNullOrWhiteSpace(user.FullName) && hasPan
            };
        }

        public async Task<bool> UpdateProfileAsync(Guid userId, UpdateUserProfileDto profileDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.FullName = profileDto.FullName.Trim();
            var normalizedPan = profileDto.PanCardNumber.Trim().ToUpperInvariant();
            var encryptedPan = normalizedPan.Encrypt();
            user.PanCardNumber_Encrypted = Encoding.UTF8.GetBytes(encryptedPan);
            user.UpdatedAtUTC = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return true;
        }
    }
}

