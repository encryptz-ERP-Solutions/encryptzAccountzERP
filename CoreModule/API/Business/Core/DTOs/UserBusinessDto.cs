using System;

namespace BusinessLogic.Core.DTOs
{
    public class UserBusinessDto
    {
        public Guid UserBusinessID { get; set; }
        public Guid UserID { get; set; }
        public Guid BusinessID { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAtUTC { get; set; }
        public DateTime? UpdatedAtUTC { get; set; }
        public string? BusinessName { get; set; }
        public string? BusinessCode { get; set; }
    }

    public class CreateUserBusinessRequest
    {
        public Guid UserID { get; set; }
        public Guid BusinessID { get; set; }
        public bool? IsDefault { get; set; }
    }

    public class SetDefaultBusinessRequest
    {
        public Guid UserBusinessID { get; set; }
    }
}

