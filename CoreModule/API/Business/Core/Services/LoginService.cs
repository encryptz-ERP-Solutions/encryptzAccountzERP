using System;
using System.Threading.Tasks;
using BusinessLogic.Admin.Interface;
using BusinessLogic.Admin.Services;
using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Infrastructure.Jwt;
using Repository.Admin.Interface;
using Repository.Core.Interface;

namespace BusinessLogic.Core.Services
{
    public class LoginService : ILoginService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILoginRepository _loginRepository;
        private readonly TokenService _tokenService;

        public LoginService(IUserRepository userRepository, ILoginRepository loginRepository, TokenService tokenService)
        {
            _userRepository = userRepository;
            _loginRepository = loginRepository;
            _tokenService = tokenService;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest)
        {
            // Determine if the identifier is an email or a user handle
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

            // At this point, login is successful. Generate token.
            var (tokenString, expiration) = _tokenService.GenerateAccessToken(user.UserID.ToString(), user.UserHandle, "User"); // Role can be expanded later

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
                return false; // User not found or has no password set
            }

            // Verify the user's old password
            if (!PasswordHasher.VerifyPassword(changePasswordDto.OldPassword, user.HashedPassword))
            {
                return false; // Old password does not match
            }

            // Hash and update the new password
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
                // Do not reveal that the user does not exist for security reasons.
                // Pretend to send the email/SMS anyway.
                return true;
            }

            var otp = new Random().Next(100000, 999999).ToString();
            await _loginRepository.SaveOTPAsync(forgotPasswordRequestDto.LoginIdentifier, otp);

            // TODO: Integrate a real email/SMS service here to send the OTP.
            // For now, the process is considered successful if the OTP is saved.

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var isOtpValid = await _loginRepository.VerifyOTPAsync(resetPasswordDto.LoginIdentifier, resetPasswordDto.Otp);
            if (!isOtpValid)
            {
                return false; // OTP is invalid, expired, or already used.
            }

            var isEmail = resetPasswordDto.LoginIdentifier.Contains('@');
            var user = isEmail
                ? await _userRepository.GetByEmailAsync(resetPasswordDto.LoginIdentifier)
                : await _userRepository.GetByUserHandleAsync(resetPasswordDto.LoginIdentifier);

            if (user == null)
            {
                // This case should be rare if OTP verification passed, but handle it anyway.
                return false;
            }

            var newHashedPassword = PasswordHasher.HashPassword(resetPasswordDto.NewPassword);
            return await _loginRepository.ChangePasswordAsync(user.UserID, newHashedPassword);
        }

        public async Task<bool> RequestLoginOtpAsync(OtpLoginRequestDto request)
        {
            var otp = new Random().Next(100000, 999999).ToString();
            await _loginRepository.SaveOTPAsync(request.LoginIdentifier, otp);

            // TODO: Integrate a real email/SMS service here to send the OTP.
            // For now, always return true to prevent user enumeration.
            return true;
        }

        public async Task<LoginResponseDto> VerifyLoginOtpAsync(OtpLoginVerifyDto request)
        {
            var isOtpValid = await _loginRepository.VerifyOTPAsync(request.LoginIdentifier, request.Otp);
            if (!isOtpValid)
            {
                return new LoginResponseDto { IsSuccess = false, Message = "Invalid or expired OTP." };
            }

            var isEmail = request.LoginIdentifier.Contains('@');
            var user = isEmail
                ? await _userRepository.GetByEmailAsync(request.LoginIdentifier)
                : await _userRepository.GetByUserHandleAsync(request.LoginIdentifier);

            // If user does not exist, create them on-the-fly
            if (user == null)
            {
                var newUser = new Entities.Admin.User
                {
                    UserID = Guid.NewGuid(),
                    Email = isEmail ? request.LoginIdentifier : null,
                    UserHandle = isEmail ? request.LoginIdentifier.Split('@')[0] : request.LoginIdentifier,
                    FullName = isEmail ? request.LoginIdentifier.Split('@')[0] : request.LoginIdentifier, // Default full name
                    IsActive = true,
                    CreatedAtUTC = DateTime.UtcNow,
                    UpdatedAtUTC = DateTime.UtcNow
                };
                user = await _userRepository.AddAsync(newUser);
            }

            if (!user.IsActive)
            {
                return new LoginResponseDto { IsSuccess = false, Message = "User account is inactive." };
            }

            // Login is successful, generate token
            var (tokenString, expiration) = _tokenService.GenerateAccessToken(user.UserID.ToString(), user.UserHandle, "User");

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
    }
}