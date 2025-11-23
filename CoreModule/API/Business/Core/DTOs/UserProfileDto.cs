using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Core.DTOs
{
    public class UserProfileDto
    {
        public Guid UserId { get; set; }
        public string UserHandle { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? MobileCountryCode { get; set; }
        public string? MobileNumber { get; set; }
        public bool HasPanCard { get; set; }
        public bool IsProfileComplete { get; set; }
    }

    public class UpdateUserProfileDto
    {
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^[A-Z]{5}[0-9]{4}[A-Z]{1}$", ErrorMessage = "PAN must follow the format AAAAA9999A.")]
        public string PanCardNumber { get; set; } = string.Empty;
    }
}

