using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Core.DTOs
{
    /// <summary>
    /// DTO for updating an existing role.
    /// </summary>
    public class RoleUpdateDto
    {
        [StringLength(100)]
        public string? RoleName { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        // A list of all permission IDs that should be associated with the role after the update.
        // The service logic will handle adding/removing permissions to match this list.
        public List<int>? PermissionIDs { get; set; }
    }
}