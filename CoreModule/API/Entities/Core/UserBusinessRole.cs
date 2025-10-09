using Entities.Admin;
using System;

namespace Entities.Core
{
    public class UserBusinessRole
    {
        public Guid UserID { get; set; }
        public Guid BusinessID { get; set; }
        public int RoleID { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
        public virtual Business Business { get; set; }
        public virtual Role Role { get; set; }
    }
}
