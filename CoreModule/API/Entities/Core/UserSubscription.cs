using System;

namespace Entities.Core
{
    public class UserSubscription
    {
        public Guid SubscriptionID { get; set; }
        public Guid BusinessID { get; set; }
        public int PlanID { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime StartDateUTC { get; set; }
        public DateTime EndDateUTC { get; set; }
        public DateTime? TrialEndsAtUTC { get; set; }
        public DateTime CreatedAtUTC { get; set; }
        public DateTime UpdatedAtUTC { get; set; }

        // Navigation properties
        public virtual Business? Business { get; set; }
        public virtual SubscriptionPlan? Plan { get; set; }
    }
}