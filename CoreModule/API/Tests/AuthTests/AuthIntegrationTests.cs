using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessLogic.Core.DTOs.Auth;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AuthTests
{
    /// <summary>
    /// Integration tests for authentication flows
    /// </summary>
    public class AuthIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public AuthIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task RegisterLoginAndAccessProtectedEndpoint_ShouldSucceed()
        {
            // Arrange
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var registerRequest = new RegisterRequestDto
            {
                UserHandle = $"testuser_{uniqueId}",
                FullName = "Test User",
                Email = $"test_{uniqueId}@example.com",
                Password = "Test@1234"
            };

            // Act 1: Register a new user
            var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
            
            // Assert: Registration should succeed
            Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
            
            var registerContent = await registerResponse.Content.ReadAsStringAsync();
            var registerResult = JsonSerializer.Deserialize<JsonElement>(registerContent);
            Assert.True(registerResult.TryGetProperty("accessToken", out var accessTokenProp));
            var accessToken = accessTokenProp.GetString();
            Assert.NotNull(accessToken);
            Assert.NotEmpty(accessToken);

            // Extract refresh token from cookie
            Assert.True(registerResponse.Headers.TryGetValues("Set-Cookie", out var cookies));
            var refreshTokenCookie = GetRefreshTokenFromCookies(cookies);
            Assert.NotNull(refreshTokenCookie);

            // Act 2: Access protected endpoint /users/me
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var meResponse = await _client.GetAsync("/api/v1/auth/users/me");
            
            // Assert: Should access protected endpoint successfully
            Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
            
            var meContent = await meResponse.Content.ReadAsStringAsync();
            var meResult = JsonSerializer.Deserialize<JsonElement>(meContent);
            Assert.True(meResult.TryGetProperty("userId", out _));
            Assert.True(meResult.TryGetProperty("userHandle", out var userHandleProp));
            Assert.Equal(registerRequest.UserHandle, userHandleProp.GetString());

            // Act 3: Login with the same credentials
            var loginRequest = new LoginRequestDto
            {
                EmailOrUserHandle = registerRequest.Email,
                Password = registerRequest.Password
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
            
            // Assert: Login should succeed
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<JsonElement>(loginContent);
            Assert.True(loginResult.TryGetProperty("accessToken", out var loginAccessTokenProp));
            Assert.NotNull(loginAccessTokenProp.GetString());
        }

        [Fact]
        public async Task LoginAndRefresh_OldRefreshTokenShouldBeRevoked()
        {
            // Arrange: Create and login a user
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var registerRequest = new RegisterRequestDto
            {
                UserHandle = $"refreshuser_{uniqueId}",
                FullName = "Refresh Test User",
                Email = $"refresh_{uniqueId}@example.com",
                Password = "Test@1234"
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
            Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

            // Get the first refresh token from cookie
            Assert.True(registerResponse.Headers.TryGetValues("Set-Cookie", out var firstCookies));
            var firstRefreshToken = GetRefreshTokenFromCookies(firstCookies);
            Assert.NotNull(firstRefreshToken);

            // Act 1: Refresh the token
            var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/refresh");
            refreshRequest.Headers.Add("Cookie", $"refreshToken={firstRefreshToken}");
            
            var refreshResponse = await _client.SendAsync(refreshRequest);
            
            // Assert: Refresh should succeed
            Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
            
            // Get the new refresh token
            Assert.True(refreshResponse.Headers.TryGetValues("Set-Cookie", out var secondCookies));
            var secondRefreshToken = GetRefreshTokenFromCookies(secondCookies);
            Assert.NotNull(secondRefreshToken);
            Assert.NotEqual(firstRefreshToken, secondRefreshToken);

            // Act 2: Try to use the old refresh token again
            var oldTokenRefreshRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/refresh");
            oldTokenRefreshRequest.Headers.Add("Cookie", $"refreshToken={firstRefreshToken}");
            
            var oldTokenResponse = await _client.SendAsync(oldTokenRefreshRequest);
            
            // Assert: Using old token should fail (token is revoked/rotated)
            Assert.Equal(HttpStatusCode.Unauthorized, oldTokenResponse.StatusCode);
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
        {
            // Arrange: Create a user first
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var registerRequest = new RegisterRequestDto
            {
                UserHandle = $"invalidpwd_{uniqueId}",
                FullName = "Invalid Password User",
                Email = $"invalidpwd_{uniqueId}@example.com",
                Password = "Correct@1234"
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
            Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

            // Act: Try to login with wrong password
            var loginRequest = new LoginRequestDto
            {
                EmailOrUserHandle = registerRequest.Email,
                Password = "WrongPassword123!"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

            // Assert: Should return Unauthorized
            Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
        }

        [Fact]
        public async Task Register_WithWeakPassword_ShouldReturnBadRequest()
        {
            // Arrange
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var registerRequest = new RegisterRequestDto
            {
                UserHandle = $"weakpwd_{uniqueId}",
                FullName = "Weak Password User",
                Email = $"weakpwd_{uniqueId}@example.com",
                Password = "weak" // No uppercase, no special char, too short
            };

            // Act
            var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

            // Assert: Should return BadRequest
            Assert.Equal(HttpStatusCode.BadRequest, registerResponse.StatusCode);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
        {
            // Arrange: Create a user first
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var email = $"duplicate_{uniqueId}@example.com";
            
            var firstRegisterRequest = new RegisterRequestDto
            {
                UserHandle = $"user1_{uniqueId}",
                FullName = "First User",
                Email = email,
                Password = "Test@1234"
            };

            var firstResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", firstRegisterRequest);
            Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

            // Act: Try to register with same email
            var secondRegisterRequest = new RegisterRequestDto
            {
                UserHandle = $"user2_{uniqueId}",
                FullName = "Second User",
                Email = email, // Same email
                Password = "Test@5678"
            };

            var secondResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", secondRegisterRequest);

            // Assert: Should return BadRequest
            Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);
        }

        [Fact]
        public async Task Logout_ShouldRevokeAllTokens()
        {
            // Arrange: Create and login a user
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var registerRequest = new RegisterRequestDto
            {
                UserHandle = $"logoutuser_{uniqueId}",
                FullName = "Logout Test User",
                Email = $"logout_{uniqueId}@example.com",
                Password = "Test@1234"
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
            Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
            
            var registerContent = await registerResponse.Content.ReadAsStringAsync();
            var registerResult = JsonSerializer.Deserialize<JsonElement>(registerContent);
            var accessToken = registerResult.GetProperty("accessToken").GetString();
            Assert.NotNull(accessToken);

            Assert.True(registerResponse.Headers.TryGetValues("Set-Cookie", out var cookies));
            var refreshToken = GetRefreshTokenFromCookies(cookies);

            // Act: Logout
            var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
            logoutRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            
            var logoutResponse = await _client.SendAsync(logoutRequest);
            
            // Assert: Logout should succeed
            Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);

            // Try to use the refresh token after logout
            var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/refresh");
            refreshRequest.Headers.Add("Cookie", $"refreshToken={refreshToken}");
            
            var refreshResponse = await _client.SendAsync(refreshRequest);
            
            // Assert: Refresh should fail (all tokens revoked)
            Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
        }

        [Fact]
        public async Task AccessProtectedEndpoint_WithoutToken_ShouldReturnUnauthorized()
        {
            // Act: Try to access protected endpoint without authentication
            var response = await _client.GetAsync("/api/v1/auth/users/me");

            // Assert: Should return Unauthorized
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AccessProtectedEndpoint_WithInvalidToken_ShouldReturnUnauthorized()
        {
            // Act: Try to access with invalid token
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.token.here");
            var response = await _client.GetAsync("/api/v1/auth/users/me");

            // Assert: Should return Unauthorized
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // Helper method to extract refresh token from Set-Cookie headers
        private string? GetRefreshTokenFromCookies(IEnumerable<string> cookies)
        {
            foreach (var cookie in cookies)
            {
                if (cookie.StartsWith("refreshToken="))
                {
                    var parts = cookie.Split(';')[0].Split('=');
                    if (parts.Length >= 2)
                    {
                        return parts[1];
                    }
                }
            }
            return null;
        }
    }
}

