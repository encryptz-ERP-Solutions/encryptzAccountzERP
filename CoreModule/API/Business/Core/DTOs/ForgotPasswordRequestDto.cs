using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Core.DTOs
{
    /// <summary>
    /// DTO to initiate a password reset process.
    /// </summary>
    public class ForgotPasswordRequestDto
    {
        [Required]
        [StringLength(100)]
        public string LoginIdentifier { get; set; } = string.Empty;
    }
}