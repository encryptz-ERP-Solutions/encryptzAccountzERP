using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Core.DTOs
{
    public class OtpVerifyDto
    {
        [Required]
        [StringLength(100)]
        public string LoginIdentifier { get; set; } = string.Empty;

        [Required]
        public string Otp { get; set; } = string.Empty;

        // Optional fields for first-time login to complete profile
        [StringLength(200)]
        public string? FullName { get; set; }

        [RegularExpression("^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "PAN must follow the format AAAAA9999A.")]
        public string? PanCardNumber { get; set; }
    }
}