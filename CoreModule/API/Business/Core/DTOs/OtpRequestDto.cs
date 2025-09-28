using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Core.DTOs
{
    public class OtpRequestDto
    {
        [Required]
        public string LoginIdentifier { get; set; }

        [Required]
        public string OtpMethod { get; set; }
    }
}