using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Core
{
    public class SubscriptionPlan
    {
        public int PlanID { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        public int MaxUsers { get; set; }
        public int MaxBusinesses { get; set; }
        public bool IsPubliclyVisible { get; set; }
        public bool IsActive { get; set; }

        // Navigation properties
        public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
        public virtual ICollection<SubscriptionPlanPermission> SubscriptionPlanPermissions { get; set; } = new List<SubscriptionPlanPermission>();
    }
}