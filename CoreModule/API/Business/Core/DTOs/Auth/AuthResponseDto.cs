using System;

namespace BusinessLogic.Core.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiresAt { get; set; }
        public DateTime RefreshTokenExpiresAt { get; set; }
        public string UserHandle { get; set; } = string.Empty;
        public Guid UserId { get; set; }
    }
}

