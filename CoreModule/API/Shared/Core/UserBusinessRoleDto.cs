using System;

namespace Shared.Core
{
    public class UserBusinessRoleDto
    {
        public Guid UserID { get; set; }
        public Guid BusinessID { get; set; }
        public int RoleID { get; set; }
    }
}
