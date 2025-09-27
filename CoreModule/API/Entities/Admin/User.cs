using System;
using System.Collections.Generic;
using Entities.Core;

namespace Entities.Admin
{
    public class User
    {
        public Guid UserID { get; set; }
        public string UserHandle { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? HashedPassword { get; set; }
        public string? MobileCountryCode { get; set; }
        public string? MobileNumber { get; set; }
        public byte[]? PanCardNumber_Encrypted { get; set; }
        public byte[]? AadharNumber_Encrypted { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAtUTC { get; set; }
        public DateTime UpdatedAtUTC { get; set; }

        // Navigation properties
        public virtual ICollection<UserBusinessRole> UserBusinessRoles { get; set; } = new List<UserBusinessRole>();
        public virtual ICollection<Business> CreatedBusinesses { get; set; } = new List<Business>();
        public virtual ICollection<Business> UpdatedBusinesses { get; set; } = new List<Business>();
    }
}