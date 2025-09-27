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

            // At this point, login is successful. Fetch permissions and generate token.
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

            // If the identifier is an email, send the OTP.
            if (isEmail)
            {
                await _emailService.SendEmail(user.Email, otp, user.FullName);
            }

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

            if (request.OtpMethod.Equals("email", StringComparison.OrdinalIgnoreCase))
            {
                var isEmail = request.LoginIdentifier.Contains('@');
                if (!isEmail)
                {
                    // Cannot send email if identifier is not an email address.
                    // Silently succeed to prevent user enumeration.
                    return true;
                }

                var user = await _userRepository.GetByEmailAsync(request.LoginIdentifier);
                var fullName = user?.FullName ?? request.LoginIdentifier; // Use identifier as fallback name

                await _emailService.SendEmail(request.LoginIdentifier, otp, fullName);
            }

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

            // Login is successful, fetch permissions and generate token
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
    }
}