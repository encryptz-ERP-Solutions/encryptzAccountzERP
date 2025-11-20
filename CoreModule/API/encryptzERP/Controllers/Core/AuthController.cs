using System;
using System.Threading.Tasks;
using BusinessLogic.Core.DTOs.Auth;
using BusinessLogic.Core.Services.Auth;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace encryptzERP.Controllers.Core
{
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ExceptionHandler _exceptionHandler;
        private const string RefreshTokenCookieName = "refreshToken";

        public AuthController(IAuthService authService, ExceptionHandler exceptionHandler)
        {
            _authService = authService;
            _exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Register a new user account
        /// </summary>
        /// <param name="request">Registration details</param>
        /// <returns>JWT access token and refresh token</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var ipAddress = GetIpAddress();
                var response = await _authService.RegisterAsync(request, ipAddress);

                // Set refresh token in HTTP-only cookie
                SetRefreshTokenCookie(response.RefreshToken, response.RefreshTokenExpiresAt);

                // Return access token in response body (don't include refresh token in body)
                return Ok(new
                {
                    accessToken = response.AccessToken,
                    expiresAt = response.AccessTokenExpiresAt,
                    userId = response.UserId,
                    userHandle = response.UserHandle
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, new { message = "An error occurred during registration." });
            }
        }

        /// <summary>
        /// Login with email/username and password
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>JWT access token and refresh token</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var ipAddress = GetIpAddress();
                var response = await _authService.LoginAsync(request, ipAddress);

                // Set refresh token in HTTP-only cookie
                SetRefreshTokenCookie(response.RefreshToken, response.RefreshTokenExpiresAt);

                // Return access token in response body
                return Ok(new
                {
                    accessToken = response.AccessToken,
                    expiresAt = response.AccessTokenExpiresAt,
                    userId = response.UserId,
                    userHandle = response.UserHandle
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, new { message = "An error occurred during login." });
            }
        }

        /// <summary>
        /// Refresh access token using refresh token (with rotation)
        /// </summary>
        /// <param name="request">Optional refresh token in body (if not using cookies)</param>
        /// <returns>New JWT access token and new refresh token</returns>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto? request = null)
        {
            try
            {
                // Try to get refresh token from cookie first, then from request body
                var refreshToken = Request.Cookies[RefreshTokenCookieName] ?? request?.RefreshToken;

                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    return BadRequest(new { message = "Refresh token is required." });
                }

                var ipAddress = GetIpAddress();
                var response = await _authService.RefreshAsync(refreshToken, ipAddress);

                // Set new refresh token in HTTP-only cookie
                SetRefreshTokenCookie(response.RefreshToken, response.RefreshTokenExpiresAt);

                // Return new access token
                return Ok(new
                {
                    accessToken = response.AccessToken,
                    expiresAt = response.AccessTokenExpiresAt,
                    userId = response.UserId,
                    userHandle = response.UserHandle
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, new { message = "An error occurred during token refresh." });
            }
        }

        /// <summary>
        /// Logout and revoke all user's refresh tokens
        /// </summary>
        /// <returns>Success message</returns>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid user token." });
                }

                // Revoke all user's refresh tokens
                await _authService.LogoutAsync(userId);

                // Clear refresh token cookie
                Response.Cookies.Delete(RefreshTokenCookieName);

                return Ok(new { message = "Logged out successfully." });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, new { message = "An error occurred during logout." });
            }
        }

        /// <summary>
        /// Revoke a specific refresh token
        /// </summary>
        /// <param name="request">Refresh token to revoke</param>
        /// <returns>Success message</returns>
        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> Revoke([FromBody] RevokeRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.RefreshToken))
                {
                    return BadRequest(new { message = "Refresh token is required." });
                }

                var ipAddress = GetIpAddress();
                var success = await _authService.RevokeAsync(request.RefreshToken, ipAddress);

                if (!success)
                {
                    return BadRequest(new { message = "Token revocation failed. Token may be invalid or already revoked." });
                }

                return Ok(new { message = "Token revoked successfully." });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, new { message = "An error occurred during token revocation." });
            }
        }

        #region Helper Methods

        /// <summary>
        /// Sets the refresh token in an HTTP-only, secure cookie
        /// </summary>
        private void SetRefreshTokenCookie(string refreshToken, DateTime expiresAt)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Set to true in production (HTTPS only)
                SameSite = SameSiteMode.Strict,
                Expires = expiresAt,
                Path = "/api/v1/auth" // Limit cookie scope to auth endpoints
            };

            Response.Cookies.Append(RefreshTokenCookieName, refreshToken, cookieOptions);
        }

        /// <summary>
        /// Gets the client IP address from the request
        /// </summary>
        private string? GetIpAddress()
        {
            // Check for forwarded IP first (if behind proxy/load balancer)
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        #endregion
    }
}

