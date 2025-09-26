using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Core.DTOs
{
    /// <summary>
    /// DTO to verify an OTP and complete a passwordless login.
    /// </summary>
    public class OtpLoginVerifyDto
    {
        [Required]
        [StringLength(100)]
        public string LoginIdentifier { get; set; } = string.Empty;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Otp { get; set; } = string.Empty;
    }
}