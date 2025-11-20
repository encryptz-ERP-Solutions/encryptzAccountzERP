using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Admin.Services;
using BusinessLogic.Core.DTOs.Auth;
using Entities.Admin;
using Entities.Core;
using Microsoft.Extensions.Configuration;
using Repository.Admin.Interface;
using Repository.Core.Interface;

namespace BusinessLogic.Core.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly TokenService _tokenService;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            TokenService tokenService,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, string? ipAddress = null)
        {
            // Validate password
            if (!ValidatePassword(request.Password, out var errors))
            {
                throw new InvalidOperationException($"Password validation failed: {string.Join(", ", errors)}");
            }

            // Check if user already exists
            var existingUserByEmail = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUserByEmail != null)
            {
                throw new InvalidOperationException("A user with this email already exists.");
            }

            var existingUserByHandle = await _userRepository.GetByUserHandleAsync(request.UserHandle);
            if (existingUserByHandle != null)
            {
                throw new InvalidOperationException("A user with this handle already exists.");
            }

            // Create new user
            var user = new User
            {
                UserID = Guid.NewGuid(),
                UserHandle = request.UserHandle,
                FullName = request.FullName,
                Email = request.Email,
                HashedPassword = PasswordHasher.HashPassword(request.Password),
                MobileCountryCode = request.MobileCountryCode,
                MobileNumber = request.MobileNumber,
                IsActive = true,
                CreatedAtUTC = DateTime.UtcNow
            };

            var createdUser = await _userRepository.AddAsync(user);

            // Generate tokens
            return await GenerateAuthResponseAsync(createdUser, ipAddress);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string? ipAddress = null)
        {
            // Find user by email or user handle
            User? user = null;

            if (request.EmailOrUserHandle.Contains("@"))
            {
                user = await _userRepository.GetByEmailAsync(request.EmailOrUserHandle);
            }
            else
            {
                user = await _userRepository.GetByUserHandleAsync(request.EmailOrUserHandle);
            }

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("User account is inactive.");
            }

            // Verify password
            if (string.IsNullOrEmpty(user.HashedPassword) || 
                !PasswordHasher.VerifyPassword(request.Password, user.HashedPassword))
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            // Generate tokens
            return await GenerateAuthResponseAsync(user, ipAddress);
        }

        public async Task<AuthResponseDto> RefreshAsync(string refreshToken, string? ipAddress = null)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentException("Refresh token is required.", nameof(refreshToken));
            }

            // Hash the incoming token to look up in database
            var tokenHash = HashToken(refreshToken);
            var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

            if (storedToken == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }

            if (!storedToken.IsActive)
            {
                throw new UnauthorizedAccessException("Refresh token is expired or revoked.");
            }

            // Get user
            var user = await _userRepository.GetByIdAsync(storedToken.UserID);
            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("User not found or inactive.");
            }

            // Generate new tokens
            var newAuthResponse = await GenerateAuthResponseAsync(user, ipAddress);

            // Revoke old token and set replaced_by
            var newTokenHash = HashToken(newAuthResponse.RefreshToken);
            var newStoredToken = await _refreshTokenRepository.GetByTokenHashAsync(newTokenHash);
            
            if (newStoredToken != null)
            {
                storedToken.IsRevoked = true;
                storedToken.RevokedAtUTC = DateTime.UtcNow;
                storedToken.RevokedByIP = ipAddress;
                storedToken.ReplacedByTokenID = newStoredToken.RefreshTokenID;
                await _refreshTokenRepository.UpdateAsync(storedToken);
            }

            return newAuthResponse;
        }

        public async Task<bool> RevokeAsync(string refreshToken, string? ipAddress = null)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return false;
            }

            var tokenHash = HashToken(refreshToken);
            var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

            if (storedToken == null || !storedToken.IsActive)
            {
                return false;
            }

            return await _refreshTokenRepository.RevokeTokenAsync(storedToken.RefreshTokenID, ipAddress);
        }

        public async Task<bool> LogoutAsync(Guid userId)
        {
            return await _refreshTokenRepository.RevokeAllUserTokensAsync(userId);
        }

        public bool ValidatePassword(string password, out List<string> errors)
        {
            errors = new List<string>();

            if (string.IsNullOrWhiteSpace(password))
            {
                errors.Add("Password is required.");
                return false;
            }

            if (password.Length < 8)
            {
                errors.Add("Password must be at least 8 characters long.");
            }

            if (!password.Any(char.IsUpper))
            {
                errors.Add("Password must contain at least one uppercase letter.");
            }

            if (!password.Any(char.IsLower))
            {
                errors.Add("Password must contain at least one lowercase letter.");
            }

            if (!password.Any(char.IsDigit))
            {
                errors.Add("Password must contain at least one digit.");
            }

            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                errors.Add("Password must contain at least one special character.");
            }

            return errors.Count == 0;
        }

        public async Task<List<Claim>> GetUserClaimsAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new List<Claim>();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.UserHandle),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            };

            // Additional claims can be added here from roles/permissions
            // This is a placeholder - implement based on your permission system
            
            return claims;
        }

        private async Task<AuthResponseDto> GenerateAuthResponseAsync(User user, string? ipAddress)
        {
            // Generate access token using existing TokenService
            var (accessToken, accessTokenExpiry) = _tokenService.GenerateAccessToken(
                user.UserID.ToString(),
                user.UserHandle,
                null // You can add permissions here if needed
            );

            // Generate refresh token
            var refreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenHash = HashToken(refreshToken);

            // Get refresh token expiration from config
            var refreshTokenExpiryDays = _configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationDays", 7);
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(refreshTokenExpiryDays);

            // Store refresh token in database
            var refreshTokenEntity = new RefreshToken
            {
                RefreshTokenID = Guid.NewGuid(),
                UserID = user.UserID,
                TokenHash = refreshTokenHash,
                ExpiresAtUTC = refreshTokenExpiry,
                CreatedAtUTC = DateTime.UtcNow,
                IsRevoked = false,
                CreatedByIP = ipAddress
            };

            await _refreshTokenRepository.AddAsync(refreshTokenEntity);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = accessTokenExpiry,
                RefreshTokenExpiresAt = refreshTokenExpiry,
                UserHandle = user.UserHandle,
                UserId = user.UserID
            };
        }

        /// <summary>
        /// Hashes a refresh token using SHA256 for secure storage
        /// </summary>
        private static string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hashBytes);
        }
    }
}

