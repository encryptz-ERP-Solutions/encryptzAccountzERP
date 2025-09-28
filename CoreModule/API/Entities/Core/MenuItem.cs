using System.Collections.Generic;

namespace Entities.Core
{
    public class MenuItem
    {
        public int MenuItemID { get; set; }
        public int ModuleID { get; set; }
        public int? ParentMenuItemID { get; set; }
        public string MenuText { get; set; } = string.Empty;
        public string? MenuURL { get; set; }
        public string? IconClass { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }

        // Navigation properties
        public virtual Module? Module { get; set; }
        public virtual MenuItem? ParentMenuItem { get; set; }
        public virtual ICollection<MenuItem> ChildMenuItems { get; set; } = new List<MenuItem>();
        public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }
}