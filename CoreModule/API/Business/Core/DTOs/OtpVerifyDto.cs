using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Core.DTOs
{
    public class OtpVerifyDto
    {
        [Required]
        [StringLength(100)]
        public string LoginIdentifier { get; set; }

        [Required]
        public string Otp { get; set; }
    }
}