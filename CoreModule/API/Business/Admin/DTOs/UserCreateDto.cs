using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Admin.DTOs
{
    /// <summary>
    /// DTO for creating a new user.
    /// </summary>
    public class UserCreateDto
    {
        [Required]
        [StringLength(50)]
        public string UserHandle { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        public string? MobileCountryCode { get; set; }

        public string? MobileNumber { get; set; }
    }
}