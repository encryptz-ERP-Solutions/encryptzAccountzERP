using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Core.DTOs
{
    public class OtpRequestDto
    {
        [Required]
        [StringLength(100)]
        public string LoginIdentifier { get; set; }

        [Required]
        public string OtpMethod { get; set; }
    }
}