using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Admin.Interface;
using BusinessLogic.Admin.Services;
using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Entities.Admin;
using Infrastructure.Extensions;
using Repository.Admin.Interface;
using Repository.Core.Interface;
using Shared.Core;

namespace BusinessLogic.Core.Services
{
    public class LoginService : ILoginService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILoginRepository _loginRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly TokenService _tokenService;
        private readonly EmailService _emailService;
        private readonly IBusinessService _businessService;
        private readonly IUserBusinessService _userBusinessService;
        private readonly IUserBusinessRoleService _userBusinessRoleService;
        private static readonly string[] AdminRoleNames = new[] { "Admin", "Business Owner" };

        public LoginService(
            IUserRepository userRepository,
            ILoginRepository loginRepository,
            IRoleRepository roleRepository,
            TokenService tokenService,
            EmailService emailService,
            IBusinessService businessService,
            IUserBusinessService userBusinessService,
            IUserBusinessRoleService userBusinessRoleService)
        {
            _userRepository = userRepository;
            _loginRepository = loginRepository;
            _roleRepository = roleRepository;
            _tokenService = tokenService;
            _emailService = emailService;
            _businessService = businessService;
            _userBusinessService = userBusinessService;
            _userBusinessRoleService = userBusinessRoleService;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest)
        {
            var isEmail = loginRequest.LoginIdentifier.Contains('@');
            var user = isEmail
                ? await _userRepository.GetByEmailAsync(loginRequest.LoginIdentifier)
                : await _userRepository.GetByUserHandleAsync(loginRequest.LoginIdentifier);

            if (user == null || !user.IsActive)
            {
                return new LoginResponseDto { IsSuccess = false, Message = "Invalid credentials or user is inactive." };
            }

            if (string.IsNullOrEmpty(user.HashedPassword) || !PasswordHasher.VerifyPassword(loginRequest.Password, user.HashedPassword))
            {
                return new LoginResponseDto { IsSuccess = false, Message = "Invalid credentials." };
            }

            // Update user information if provided during login (for users with incomplete profiles)
            bool userUpdated = false;
            if (!string.IsNullOrWhiteSpace(loginRequest.FullName) && 
                (string.IsNullOrWhiteSpace(user.FullName) || user.FullName == "New User"))
            {
                user.FullName = loginRequest.FullName.Trim();
                userUpdated = true;
            }

            if (!string.IsNullOrWhiteSpace(loginRequest.PanCardNumber))
            {
                // Check if user has placeholder PAN (GUID bytes = 16 bytes) or no PAN
                // Encrypted PAN strings are typically much longer than 16 bytes
                bool isPlaceholderPan = user.PanCardNumber_Encrypted != null && 
                                       user.PanCardNumber_Encrypted.Length == 16; // GUID size
                
                var normalizedPan = loginRequest.PanCardNumber.Trim().ToUpperInvariant();
                var encryptedPan = normalizedPan.Encrypt();
                var encryptedPanBytes = Encoding.UTF8.GetBytes(encryptedPan);
                
                // Update PAN if:
                // 1. Current PAN is null/empty, OR
                // 2. Current PAN is a placeholder (16 bytes = GUID), OR
                // 3. New encrypted PAN is different from current
                if (user.PanCardNumber_Encrypted == null || 
                    user.PanCardNumber_Encrypted.Length == 0 ||
                    isPlaceholderPan ||
                    !encryptedPanBytes.SequenceEqual(user.PanCardNumber_Encrypted))
                {
                    user.PanCardNumber_Encrypted = encryptedPanBytes;
                    userUpdated = true;
                }
            }

            // Save updated user information if changes were made
            if (userUpdated)
            {
                user.UpdatedAtUTC = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
            }

            await EnsureDefaultBusinessAsync(user);
            var permissions = await _roleRepository.GetUserPermissionsAcrossBusinessesAsync(user.UserID);
            var isProfileComplete = IsProfileComplete(user);
            var isSystemAdmin = await _roleRepository.UserHasAnyRoleAsync(user.UserID, AdminRoleNames);
            var additionalClaims = BuildAdditionalClaims(isSystemAdmin, isProfileComplete);
            var (tokenString, expiration) = _tokenService.GenerateAccessToken(
                user.UserID.ToString(),
                user.UserHandle,
                permissions,
                additionalClaims);

            return new LoginResponseDto
            {
                IsSuccess = true,
                Message = "Login successful.",
                Token = tokenString,
                TokenExpiration = expiration,
                UserId = user.UserID,
                UserHandle = user.UserHandle,
                FullName = user.FullName
            };
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.HashedPassword))
            {
                return false;
            }

            if (!PasswordHasher.VerifyPassword(changePasswordDto.OldPassword, user.HashedPassword))
            {
                return false;
            }

            var newHashedPassword = PasswordHasher.HashPassword(changePasswordDto.NewPassword);
            return await _loginRepository.ChangePasswordAsync(userId, newHashedPassword);
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequestDto forgotPasswordRequestDto)
        {
            var isEmail = forgotPasswordRequestDto.LoginIdentifier.Contains('@');
            var user = isEmail
                ? await _userRepository.GetByEmailAsync(forgotPasswordRequestDto.LoginIdentifier)
                : await _userRepository.GetByUserHandleAsync(forgotPasswordRequestDto.LoginIdentifier);

            if (user == null)
            {
                return true; // Silently succeed
            }

            var otp = new Random().Next(100000, 999999).ToString();
            await _loginRepository.SaveOTPAsync(forgotPasswordRequestDto.LoginIdentifier, otp);

            if (isEmail && !string.IsNullOrEmpty(user.Email))
            {
                await _emailService.SendEmail(user.Email, otp, user.FullName);
            }

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            if (!await _loginRepository.VerifyOTPAsync(resetPasswordDto.LoginIdentifier, resetPasswordDto.Otp))
            {
                return false;
            }

            var isEmail = resetPasswordDto.LoginIdentifier.Contains('@');
            var user = isEmail
                ? await _userRepository.GetByEmailAsync(resetPasswordDto.LoginIdentifier)
                : await _userRepository.GetByUserHandleAsync(resetPasswordDto.LoginIdentifier);

            if (user == null)
            {
                return false;
            }

            var newHashedPassword = PasswordHasher.HashPassword(resetPasswordDto.NewPassword);
            return await _loginRepository.ChangePasswordAsync(user.UserID, newHashedPassword);
        }

        public async Task<bool> RequestOtpAsync(OtpRequestDto otpRequestDto)
        {
            var isEmail = otpRequestDto.LoginIdentifier.Contains('@');
            var user = isEmail
                ? await _userRepository.GetByEmailAsync(otpRequestDto.LoginIdentifier)
                : await _userRepository.GetByUserHandleAsync(otpRequestDto.LoginIdentifier);

            if (user == null)
            {
                // Create a dummy user if one doesn't exist
                // Generate unique PAN card number to avoid duplicate key violations
                var uniquePanBytes = Guid.NewGuid().ToByteArray();
                
                var newUser = new User
                {
                    UserID = Guid.NewGuid(),
                    FullName = "New User",
                    UserHandle = isEmail ? otpRequestDto.LoginIdentifier.Split('@').FirstOrDefault() ?? $"user_{new Random().Next(1000, 9999)}" : otpRequestDto.LoginIdentifier,
                    Email = isEmail ? otpRequestDto.LoginIdentifier : null,
                    IsActive = true, // Or false, depending on business logic for new users
                    CreatedAtUTC = DateTime.UtcNow,
                    PanCardNumber_Encrypted = uniquePanBytes, // Use unique bytes to avoid duplicate hash constraint violation
                    AadharNumber_Encrypted = null,
                    HashedPassword = PasswordHasher.HashPassword("WelcomePassword"), // Set a default password or handle differently
                    MobileCountryCode = null,
                    MobileNumber = null,
                    UpdatedAtUTC = null
                    
                };
                user = await _userRepository.AddAsync(newUser);
            }

            var otp = new Random().Next(100000, 999999).ToString();
            await _loginRepository.SaveOTPAsync(otpRequestDto.LoginIdentifier, otp);

            if (otpRequestDto.OtpMethod.Equals("email", StringComparison.OrdinalIgnoreCase) && isEmail && !string.IsNullOrEmpty(user.Email))
            {
                await _emailService.SendEmail(user.Email, otp, user.FullName);
            }

            return true;
        }

        public async Task<LoginResponseDto> VerifyOtpAsync(OtpVerifyDto otpVerifyDto)
        {
            var isValidOtp = await _loginRepository.VerifyOTPAsync(otpVerifyDto.LoginIdentifier, otpVerifyDto.Otp);
            if (!isValidOtp)
            {
                return new LoginResponseDto { IsSuccess = false, Message = "Invalid OTP." };
            }

            var isEmail = otpVerifyDto.LoginIdentifier.Contains('@');
            var user = isEmail
                ? await _userRepository.GetByEmailAsync(otpVerifyDto.LoginIdentifier)
                : await _userRepository.GetByUserHandleAsync(otpVerifyDto.LoginIdentifier);

            if (user == null)
            {
                return new LoginResponseDto { IsSuccess = false, Message = "User not found." };
            }

            // Update user information if provided during first-time login
            bool userUpdated = false;
            if (!string.IsNullOrWhiteSpace(otpVerifyDto.FullName) && 
                (string.IsNullOrWhiteSpace(user.FullName) || user.FullName == "New User"))
            {
                user.FullName = otpVerifyDto.FullName.Trim();
                userUpdated = true;
            }

            if (!string.IsNullOrWhiteSpace(otpVerifyDto.PanCardNumber))
            {
                // Check if user has placeholder PAN (GUID bytes = 16 bytes) or no PAN
                // Encrypted PAN strings are typically much longer than 16 bytes
                bool isPlaceholderPan = user.PanCardNumber_Encrypted != null && 
                                       user.PanCardNumber_Encrypted.Length == 16; // GUID size
                
                var normalizedPan = otpVerifyDto.PanCardNumber.Trim().ToUpperInvariant();
                var encryptedPan = normalizedPan.Encrypt();
                var encryptedPanBytes = Encoding.UTF8.GetBytes(encryptedPan);
                
                // Update PAN if:
                // 1. Current PAN is null/empty, OR
                // 2. Current PAN is a placeholder (16 bytes = GUID), OR
                // 3. New encrypted PAN is different from current
                if (user.PanCardNumber_Encrypted == null || 
                    user.PanCardNumber_Encrypted.Length == 0 ||
                    isPlaceholderPan ||
                    !encryptedPanBytes.SequenceEqual(user.PanCardNumber_Encrypted))
                {
                    user.PanCardNumber_Encrypted = encryptedPanBytes;
                    userUpdated = true;
                }
            }

            // Save updated user information if changes were made
            if (userUpdated)
            {
                user.UpdatedAtUTC = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
            }

            await EnsureDefaultBusinessAsync(user);
            var permissions = await _roleRepository.GetUserPermissionsAcrossBusinessesAsync(user.UserID);
            var isProfileComplete = IsProfileComplete(user);
            var isSystemAdmin = await _roleRepository.UserHasAnyRoleAsync(user.UserID, AdminRoleNames);
            var additionalClaims = BuildAdditionalClaims(isSystemAdmin, isProfileComplete);
            var (tokenString, expiration) = _tokenService.GenerateAccessToken(
                user.UserID.ToString(),
                user.UserHandle,
                permissions,
                additionalClaims);

            return new LoginResponseDto
            {
                IsSuccess = true,
                Message = "OTP verified successfully.",
                Token = tokenString,
                TokenExpiration = expiration,
                UserId = user.UserID,
                UserHandle = user.UserHandle,
                FullName = user.FullName
            };
        }
        private async Task EnsureDefaultBusinessAsync(User user)
        {
            var existingLinks = await _userBusinessService.GetByUserIdAsync(user.UserID);
            if (existingLinks.Any())
            {
                return;
            }

            var businessName = GenerateBusinessName(user);
            var businessCode = GenerateBusinessCode(user);

            var businessDto = new BusinessDto
            {
                BusinessName = businessName,
                BusinessCode = businessCode,
                IsActive = true
            };

            var business = await _businessService.AddBusinessAsync(businessDto, user.UserID);

            await _userBusinessService.CreateAsync(new CreateUserBusinessRequest
            {
                UserID = user.UserID,
                BusinessID = business.BusinessID,
                IsDefault = true
            }, user.UserID);

            var ownerRoleId = await _roleRepository.GetRoleIdByNameAsync("Business Owner")
                ?? await _roleRepository.GetRoleIdByNameAsync("Admin");

            if (ownerRoleId.HasValue)
            {
                await _userBusinessRoleService.AddAsync(new UserBusinessRoleDto
                {
                    UserID = user.UserID,
                    BusinessID = business.BusinessID,
                    RoleID = ownerRoleId.Value
                });
            }
        }

        private static string GenerateBusinessName(User user)
        {
            if (!string.IsNullOrWhiteSpace(user.FullName))
            {
                return $"{user.FullName}'s Workspace";
            }

            return $"{user.UserHandle}'s Workspace";
        }

        private static string GenerateBusinessCode(User user)
        {
            var baseCode = string.IsNullOrWhiteSpace(user.UserHandle)
                ? "AUTO"
                : new string(user.UserHandle.Where(char.IsLetterOrDigit).ToArray());

            if (string.IsNullOrWhiteSpace(baseCode))
            {
                baseCode = "AUTO";
            }

            baseCode = baseCode.ToUpperInvariant();
            var suffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            return $"{baseCode}-{suffix}";
        }

        private static bool IsProfileComplete(User user)
        {
            return !string.IsNullOrWhiteSpace(user.FullName) &&
                   user.PanCardNumber_Encrypted != null &&
                   user.PanCardNumber_Encrypted.Length > 0;
        }

        private static Dictionary<string, string> BuildAdditionalClaims(bool isSystemAdmin, bool isProfileComplete)
        {
            return new Dictionary<string, string>
            {
                { "is_admin", isSystemAdmin ? "true" : "false" },
                { "profile_complete", isProfileComplete ? "true" : "false" }
            };
        }
    }
}