using System.Collections.Generic;

namespace BusinessLogic.Core.DTOs
{
    /// <summary>
    /// Represents a role and its associated permissions.
    /// </summary>
    public class RoleDto
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsSystemRole { get; set; }
        public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
    }
}