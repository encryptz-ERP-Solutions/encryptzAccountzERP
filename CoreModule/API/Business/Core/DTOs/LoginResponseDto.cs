using System;

namespace BusinessLogic.Core.DTOs
{
    /// <summary>
    /// DTO for a successful login response.
    /// </summary>
    public class LoginResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public DateTime? TokenExpiration { get; set; }
        public Guid? UserId { get; set; }
        public string? UserHandle { get; set; }
        public string? FullName { get; set; }
    }
}