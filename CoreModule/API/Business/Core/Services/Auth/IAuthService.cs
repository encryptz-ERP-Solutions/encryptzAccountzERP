using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BusinessLogic.Core.DTOs.Auth;

namespace BusinessLogic.Core.Services.Auth
{
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user and returns authentication tokens
        /// </summary>
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, string? ipAddress = null);

        /// <summary>
        /// Authenticates a user and returns JWT access token + refresh token
        /// </summary>
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string? ipAddress = null);

        /// <summary>
        /// Refreshes the access token using a valid refresh token (with rotation)
        /// </summary>
        Task<AuthResponseDto> RefreshAsync(string refreshToken, string? ipAddress = null);

        /// <summary>
        /// Revokes a specific refresh token
        /// </summary>
        Task<bool> RevokeAsync(string refreshToken, string? ipAddress = null);

        /// <summary>
        /// Logs out user by revoking all their refresh tokens
        /// </summary>
        Task<bool> LogoutAsync(Guid userId);

        /// <summary>
        /// Validates a password against security requirements
        /// </summary>
        bool ValidatePassword(string password, out List<string> errors);

        /// <summary>
        /// Gets user claims for JWT token generation
        /// </summary>
        Task<List<Claim>> GetUserClaimsAsync(Guid userId);
    }
}

