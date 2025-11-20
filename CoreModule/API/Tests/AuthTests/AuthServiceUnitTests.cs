using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.Admin.Services;
using BusinessLogic.Core.DTOs.Auth;
using BusinessLogic.Core.Services.Auth;
using Entities.Admin;
using Entities.Core;
using Microsoft.Extensions.Configuration;
using Moq;
using Repository.Admin.Interface;
using Repository.Core.Interface;
using Xunit;

namespace AuthTests
{
    /// <summary>
    /// Unit tests for AuthService
    /// </summary>
    public class AuthServiceUnitTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
        private readonly Mock<TokenService> _mockTokenService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly IAuthService _authService;

        public AuthServiceUnitTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();
            _mockConfiguration = new Mock<IConfiguration>();
            
            // Setup configuration mock
            _mockConfiguration.Setup(c => c["JwtSettings:SecretKey"]).Returns("TestSecretKeyThatIsLongEnoughForHMACSHA256Algorithm123456");
            _mockConfiguration.Setup(c => c["JwtSettings:Issuer"]).Returns("TestIssuer");
            _mockConfiguration.Setup(c => c["JwtSettings:Audience"]).Returns("TestAudience");
            _mockConfiguration.Setup(c => c["JwtSettings:AccessTokenExpirationMinutes"]).Returns("15");
            _mockConfiguration.Setup(c => c["JwtSettings:RefreshTokenExpirationDays"]).Returns("7");
            _mockConfiguration.Setup(c => c.GetValue<int>("JwtSettings:RefreshTokenExpirationDays", It.IsAny<int>())).Returns(7);

            // Create real TokenService with mocked configuration
            var tokenService = new TokenService(_mockConfiguration.Object);

            _authService = new AuthService(
                _mockUserRepository.Object,
                _mockRefreshTokenRepository.Object,
                tokenService,
                _mockConfiguration.Object
            );
        }

        #region RegisterAsync Tests

        [Fact]
        public async Task RegisterAsync_WithValidData_ShouldCreateUserAndReturnTokens()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                UserHandle = "testuser",
                FullName = "Test User",
                Email = "test@example.com",
                Password = "Test@1234"
            };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);
            _mockUserRepository.Setup(r => r.GetByUserHandleAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);
            _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);
            _mockRefreshTokenRepository.Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
                .ReturnsAsync((RefreshToken t) => t);

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.AccessToken);
            Assert.NotEmpty(result.RefreshToken);
            Assert.Equal(request.UserHandle, result.UserHandle);
            _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
            _mockRefreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ShouldThrowException()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                UserHandle = "testuser",
                FullName = "Test User",
                Email = "existing@example.com",
                Password = "Test@1234"
            };

            var existingUser = new User { Email = "existing@example.com" };
            _mockUserRepository.Setup(r => r.GetByEmailAsync(request.Email))
                .ReturnsAsync(existingUser);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.RegisterAsync(request));
        }

        [Fact]
        public async Task RegisterAsync_WithExistingUserHandle_ShouldThrowException()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                UserHandle = "existinghandle",
                FullName = "Test User",
                Email = "test@example.com",
                Password = "Test@1234"
            };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(request.Email))
                .ReturnsAsync((User?)null);
            _mockUserRepository.Setup(r => r.GetByUserHandleAsync(request.UserHandle))
                .ReturnsAsync(new User { UserHandle = "existinghandle" });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.RegisterAsync(request));
        }

        [Fact]
        public async Task RegisterAsync_WithWeakPassword_ShouldThrowException()
        {
            // Arrange
            var request = new RegisterRequestDto
            {
                UserHandle = "testuser",
                FullName = "Test User",
                Email = "test@example.com",
                Password = "weak"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.RegisterAsync(request));
        }

        #endregion

        #region LoginAsync Tests

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnTokens()
        {
            // Arrange
            var password = "Test@1234";
            var hashedPassword = PasswordHasher.HashPassword(password);
            
            var user = new User
            {
                UserID = Guid.NewGuid(),
                UserHandle = "testuser",
                Email = "test@example.com",
                HashedPassword = hashedPassword,
                IsActive = true
            };

            var request = new LoginRequestDto
            {
                EmailOrUserHandle = "test@example.com",
                Password = password
            };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(request.EmailOrUserHandle))
                .ReturnsAsync(user);
            _mockRefreshTokenRepository.Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
                .ReturnsAsync((RefreshToken t) => t);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.AccessToken);
            Assert.NotEmpty(result.RefreshToken);
            Assert.Equal(user.UserHandle, result.UserHandle);
            Assert.Equal(user.UserID, result.UserId);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidEmail_ShouldThrowUnauthorizedException()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                EmailOrUserHandle = "nonexistent@example.com",
                Password = "Test@1234"
            };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);
            _mockUserRepository.Setup(r => r.GetByUserHandleAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(request));
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ShouldThrowUnauthorizedException()
        {
            // Arrange
            var user = new User
            {
                UserID = Guid.NewGuid(),
                UserHandle = "testuser",
                Email = "test@example.com",
                HashedPassword = PasswordHasher.HashPassword("Correct@1234"),
                IsActive = true
            };

            var request = new LoginRequestDto
            {
                EmailOrUserHandle = "test@example.com",
                Password = "Wrong@1234"
            };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(request.EmailOrUserHandle))
                .ReturnsAsync(user);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(request));
        }

        [Fact]
        public async Task LoginAsync_WithInactiveUser_ShouldThrowUnauthorizedException()
        {
            // Arrange
            var user = new User
            {
                UserID = Guid.NewGuid(),
                UserHandle = "testuser",
                Email = "test@example.com",
                HashedPassword = PasswordHasher.HashPassword("Test@1234"),
                IsActive = false
            };

            var request = new LoginRequestDto
            {
                EmailOrUserHandle = "test@example.com",
                Password = "Test@1234"
            };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(request.EmailOrUserHandle))
                .ReturnsAsync(user);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(request));
        }

        #endregion

        #region ValidatePassword Tests

        [Theory]
        [InlineData("Test@1234", true)]
        [InlineData("ComplexP@ssw0rd!", true)]
        [InlineData("weak", false)]
        [InlineData("NoSpecialChar1", false)]
        [InlineData("nouppercas3!", false)]
        [InlineData("NOLOWERCASE1!", false)]
        [InlineData("NoDigits!", false)]
        [InlineData("Short1!", false)]
        public void ValidatePassword_ShouldReturnCorrectResult(string password, bool expectedValid)
        {
            // Act
            var isValid = _authService.ValidatePassword(password, out var errors);

            // Assert
            Assert.Equal(expectedValid, isValid);
            if (!expectedValid)
            {
                Assert.NotEmpty(errors);
            }
        }

        #endregion

        #region LogoutAsync Tests

        [Fact]
        public async Task LogoutAsync_ShouldRevokeAllUserTokens()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockRefreshTokenRepository.Setup(r => r.RevokeAllUserTokensAsync(userId))
                .ReturnsAsync(true);

            // Act
            var result = await _authService.LogoutAsync(userId);

            // Assert
            Assert.True(result);
            _mockRefreshTokenRepository.Verify(r => r.RevokeAllUserTokensAsync(userId), Times.Once);
        }

        #endregion

        #region GetUserClaimsAsync Tests

        [Fact]
        public async Task GetUserClaimsAsync_WithValidUser_ShouldReturnClaims()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                UserID = userId,
                UserHandle = "testuser",
                Email = "test@example.com",
                IsActive = true
            };

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var claims = await _authService.GetUserClaimsAsync(userId);

            // Assert
            Assert.NotNull(claims);
            Assert.NotEmpty(claims);
            Assert.Contains(claims, c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier && c.Value == userId.ToString());
            Assert.Contains(claims, c => c.Type == System.Security.Claims.ClaimTypes.Name && c.Value == "testuser");
            Assert.Contains(claims, c => c.Type == System.Security.Claims.ClaimTypes.Email && c.Value == "test@example.com");
        }

        [Fact]
        public async Task GetUserClaimsAsync_WithNonExistentUser_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            var claims = await _authService.GetUserClaimsAsync(userId);

            // Assert
            Assert.NotNull(claims);
            Assert.Empty(claims);
        }

        #endregion
    }
}

