using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Core.DTOs
{
    /// <summary>
    /// DTO to request an OTP for passwordless login.
    /// </summary>
    public class OtpLoginRequestDto
    {
        [Required]
        [StringLength(100)]
        public string LoginIdentifier { get; set; } = string.Empty;
    }
}