using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Admin.DTOs
{
    /// <summary>
    /// DTO for updating an existing user. All properties are optional.
    /// </summary>
    public class UserUpdateDto
    {
        [StringLength(100)]
        public string? FullName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? MobileCountryCode { get; set; }

        public string? MobileNumber { get; set; }

        public bool? IsActive { get; set; }
    }
}