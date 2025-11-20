using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Core.DTOs.Auth
{
    public class RefreshRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}

