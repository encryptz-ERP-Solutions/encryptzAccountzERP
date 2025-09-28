using System.Collections.Generic;

namespace Entities.Core
{
    public class Module
    {
        public int ModuleID { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public bool IsSystemModule { get; set; }
        public bool IsActive { get; set; }

        // Navigation properties
        public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }
}