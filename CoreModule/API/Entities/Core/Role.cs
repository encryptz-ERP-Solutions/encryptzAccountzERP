using System;
using System.Collections.Generic;

namespace Entities.Core
{
    public class Role
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsSystemRole { get; set; }

        // Navigation properties
        public virtual ICollection<UserBusinessRole> UserBusinessRoles { get; set; } = new List<UserBusinessRole>();
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}