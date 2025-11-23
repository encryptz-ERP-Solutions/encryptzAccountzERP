using System;

namespace Entities.Core
{
    public class UserBusiness
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
}

