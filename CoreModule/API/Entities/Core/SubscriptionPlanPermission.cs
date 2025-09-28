namespace Entities.Core
{
    public class SubscriptionPlanPermission
    {
        public int PlanID { get; set; }
        public int PermissionID { get; set; }

        // Navigation properties
        public virtual SubscriptionPlan? Plan { get; set; }
        public virtual Permission? Permission { get; set; }
    }
}