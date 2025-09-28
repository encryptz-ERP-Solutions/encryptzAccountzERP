using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.Admin.Interface;
using BusinessLogic.Admin.Services;
using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Entities.Admin;
using Repository.Admin.Interface;
using Repository.Core.Interface;

namespace BusinessLogic.Core.Services
{
    public class LoginService : ILoginService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILoginRepository _loginRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly TokenService _tokenService;
        private readonly EmailService _emailService;

        public LoginService(
            IUserRepository userRepository,
            ILoginRepository loginRepository,
            IRoleRepository roleRepository,
            TokenService tokenService,
            EmailService emailService)
        {
            _userRepository = userRepository;
            _loginRepository = loginRepository;
            _roleRepository = roleRepository;
            _tokenService = tokenService;
            _emailService = emailService;
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

            var permissions = await _roleRepository.GetUserPermissionsAcrossBusinessesAsync(user.UserID);
            var (tokenString, expiration) = _tokenService.GenerateAccessToken(user.UserID.ToString(), user.UserHandle, permissions);

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
                // Silently succeed to prevent user enumeration attacks
                return true;
            }

            var otp = new Random().Next(100000, 999999).ToString();
            await _loginRepository.SaveOTPAsync(otpRequestDto.LoginIdentifier, otp);

            if (otpRequestDto.OtpMethod.Equals("email", StringComparison.OrdinalIgnoreCase))
            {
                if (isEmail && !string.IsNullOrEmpty(user.Email))
                {
                    await _emailService.SendEmail(user.Email, otp, user.FullName);
                }
            }

            // Logic for other OTP methods like SMS can be added here in the future.

            return true;
        }

        public async Task<bool> VerifyOtpAsync(OtpVerifyDto otpVerifyDto)
        {
            return await _loginRepository.VerifyOTPAsync(otpVerifyDto.LoginIdentifier, otpVerifyDto.Otp);
        }
    }
}