using System.Collections.Generic;

namespace Entities.Core
{
    public class Permission
    {
        public int PermissionID { get; set; }
        public string PermissionKey { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? MenuItemID { get; set; }
        public int ModuleID { get; set; }

        // Navigation properties
        public virtual MenuItem? MenuItem { get; set; }
        public virtual Module? Module { get; set; }
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public virtual ICollection<SubscriptionPlanPermission> SubscriptionPlanPermissions { get; set; } = new List<SubscriptionPlanPermission>();
    }
}