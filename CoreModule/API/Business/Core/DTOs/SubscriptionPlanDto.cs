using System;

namespace Business.Core.DTOs
{
    public enum SubscriptionStatus
    {
        Active,
        Inactive,
        Suspended,
        Cancelled,
        Expired,
        Trial,
        Pending,
        PastDue
    }

    public class SubscriptionPlanDto
    {
        public int PlanID { get; set; }
        public string PlanName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int MaxUsers { get; set; }
        public int MaxBusinesses { get; set; }
        public bool IsPubliclyVisible { get; set; }
        public bool IsActive { get; set; }
        // Audit fields
        public Guid? CreatedByUserID { get; set; }
        public DateTime? CreatedAtUTC { get; set; }
        public Guid? UpdatedByUserID { get; set; }
        public DateTime? UpdatedAtUTC { get; set; }
    }
}
