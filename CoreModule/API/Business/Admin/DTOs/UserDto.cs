using System;

namespace BusinessLogic.Admin.DTOs
{
    /// <summary>
    /// Represents a user data transfer object.
    /// </summary>
    public class UserDto
    {
        public Guid UserID { get; set; }
        public string UserHandle { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? MobileCountryCode { get; set; }
        public string? MobileNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAtUTC { get; set; }
        public DateTime UpdatedAtUTC { get; set; }
    }
}