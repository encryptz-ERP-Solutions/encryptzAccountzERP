using System;

namespace Entities.Core
{
    /// <summary>
    /// Represents a refresh token for JWT authentication with rotation support
    /// </summary>
    public class RefreshToken
    {
        public Guid RefreshTokenID { get; set; }
        public Guid UserID { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public DateTime ExpiresAtUTC { get; set; }
        public DateTime CreatedAtUTC { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAtUTC { get; set; }
        public Guid? ReplacedByTokenID { get; set; }
        public string? CreatedByIP { get; set; }
        public string? RevokedByIP { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAtUTC;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}

