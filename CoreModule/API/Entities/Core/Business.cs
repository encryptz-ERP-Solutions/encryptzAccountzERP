using System;
using System.Collections.Generic;
using Entities.Admin;

namespace Entities.Core
{
    public class Business
    {
        public Guid BusinessID { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string BusinessCode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? Gstin { get; set; }
        public string? TanNumber { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public int? StateID { get; set; }
        public string? PinCode { get; set; }
        public int? CountryID { get; set; }
        public Guid CreatedByUserID { get; set; }
        public DateTime CreatedAtUTC { get; set; }
        public Guid UpdatedByUserID { get; set; }
        public DateTime UpdatedAtUTC { get; set; }

        // Navigation properties
        public virtual User? CreatedByUser { get; set; }
        public virtual User? UpdatedByUser { get; set; }
        public virtual ICollection<UserBusinessRole> UserBusinessRoles { get; set; } = new List<UserBusinessRole>();
        public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
    }
}