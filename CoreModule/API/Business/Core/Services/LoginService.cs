using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using BusinessLogic.Admin.DTOs;
using BusinessLogic.Admin.Interface;
using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Entities.Admin;
using Microsoft.Extensions.Configuration;
using Repository.Core.Interface;

namespace BusinessLogic.Core.Services
{
    public class LoginService:ILoginService
    {
       private readonly ILoginRepository _loginRepository;
        private readonly TokenService _tokenService;
        private EmailService _emailService;
        private readonly IUserService _userService;
        private static Dictionary<string, string> _refreshTokens = new();
        private readonly IConfiguration _configuration;
        public LoginService(ILoginRepository logingRepository, TokenService tokenService, IUserService userService,IConfiguration configuration) {
            _loginRepository = logingRepository;
            _tokenService = tokenService;            
            _userService = userService;
            _configuration = configuration;
        }

       public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
        {
            try
            {
                LoginResponse response = new LoginResponse();
                if (string.IsNullOrEmpty(loginRequest.UserId) || string.IsNullOrEmpty(loginRequest.Password))
                {
                    return response;
                }

                var result = await _loginRepository.LoginAsync(loginRequest.UserId, loginRequest.Password);
                if (result.id <= 0)
                {
                    return response;
                }

                if (result.userName == "admin")
                {
                    response.Token = _tokenService.GenerateAccessToken(loginRequest.UserId, "Admin"); // Admin role
                    response.RefreshToken = _tokenService.GenerateRefreshToken();
                    return response;
                }
                else
                {
                    response.Token = _tokenService.GenerateAccessToken(loginRequest.UserId, "User"); // User role
                    response.RefreshToken = _tokenService.GenerateRefreshToken();
                }
                _refreshTokens[loginRequest.UserId] = response.RefreshToken;
                return response;

            }
            catch (Exception)
            {

                throw;
            }
        }

       public async Task<bool> LogoutAsync(string userId)
        {
            try
            {
                _refreshTokens.Remove(userId);
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            try
            {
                LoginResponse loginResponse = new LoginResponse();
                if (_refreshTokens.TryGetValue(request.UserId, out var savedRefreshToken) &&
                savedRefreshToken == request.RefreshToken)
                {
                    if (request.UserId == "admin")
                    {
                        loginResponse.Token = _tokenService.GenerateAccessToken(request.UserId, "Admin");
                    }
                    else
                    {
                        loginResponse.Token = _tokenService.GenerateAccessToken(request.UserId, "User");
                    }
                    loginResponse.RefreshToken = _tokenService.GenerateRefreshToken();

                    // Update refresh token
                    _refreshTokens[request.UserId] = loginResponse.RefreshToken;

                    return loginResponse;
                }
                return loginResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }

       public async Task<(bool, string)> SendOTP(SendOtpRequest sendOtpRequest)
        {
            try
            {
                if (sendOtpRequest.loginType == "")
                {
                    return (false, "Couldn't Identify Login Type");
                }
                if (sendOtpRequest.loginId == "")
                {
                    return (false, "Couldn't Identify Login Id");
                }

                string otp = new Random().Next(100000, 999999).ToString();

                bool response = await _loginRepository.SaveOTP(sendOtpRequest.loginType, sendOtpRequest.loginId, otp, sendOtpRequest.fullName);
                if (!response)
                {
                    return (false, $"Something went wrong. Couldn't save otp.");
                }
                if (sendOtpRequest.loginType.ToUpper() == "EMAIL")
                {
                    _emailService = new EmailService(_configuration);
                    await _emailService.SendEmail(sendOtpRequest.loginId, otp,sendOtpRequest.fullName);
                }

                return (true, $"OTP sent to {sendOtpRequest.loginId}");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<LoginResponse> VerifyOTP(VerifyOtpRequest verifyOtpRequest)
        {
            try
            {
                LoginResponse loginResponse = new LoginResponse();
                if (verifyOtpRequest.loginType == "")
                {
                    return loginResponse;
                }
                if (verifyOtpRequest.loginId == "")
                {
                    return loginResponse;
                }

                bool verfiyResponse = await _loginRepository.VerifyOTP(verifyOtpRequest.loginType, verifyOtpRequest.loginId, verifyOtpRequest.otp);
                if (!verfiyResponse)
                {
                    return loginResponse;
                }

                UserDto? existingUser = await _userService.GetUserByLoginAsync(verifyOtpRequest.loginId, verifyOtpRequest.loginType);
                if (existingUser != null)
                {
                    if (existingUser.userId != null && existingUser.userId != "")
                    {
                        loginResponse.Token = _tokenService.GenerateAccessToken(existingUser.userId, "User");
                        loginResponse.RefreshToken = _tokenService.GenerateRefreshToken();
                        _refreshTokens[existingUser.userId] = loginResponse.RefreshToken;

                        return loginResponse;
                    }
                }

                int LastUserId = await _loginRepository.GetMaxofUserId() ?? 0;
                string name = verifyOtpRequest.fullName;

                UserDto user = new UserDto();
                user.userName = name;
                user.userId = (name.Length > 3 ? name.Trim().Substring(0, 4) : name) + DateTime.Now.Year.ToString().Substring(2, 2) + LastUserId.ToString("0000");
                user.email = verifyOtpRequest.loginType.ToUpper() == "EMAIL" ? verifyOtpRequest.loginId : "";
                user.phoneNo = verifyOtpRequest.loginType.ToUpper() == "PHONE" ? verifyOtpRequest.loginId : "";
                user.panNo = verifyOtpRequest.panNo;
                user.isActive = true;
                user = await _userService.AddUserAsync(user);
                if (user != null)
                {
                    loginResponse.Token = _tokenService.GenerateAccessToken(user.userId, "User");
                }
                return loginResponse;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<bool> ChangePassword(int userId, string newPassword)
        {
            throw new NotImplementedException();
        }

        public Task<int?> GetUserIdByEmail(string email)
        {
            throw new NotImplementedException();
        }
    }
}
