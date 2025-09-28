namespace Entities.Core
{
    public class RolePermission
    {
        public int RoleID { get; set; }
        public int PermissionID { get; set; }

        // Navigation properties
        public virtual Role? Role { get; set; }
        public virtual Permission? Permission { get; set; }
    }
}