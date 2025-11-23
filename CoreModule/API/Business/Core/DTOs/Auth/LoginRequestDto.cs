using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Core.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required]
        public string EmailOrUserHandle { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}

