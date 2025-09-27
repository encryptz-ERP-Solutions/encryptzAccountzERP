using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Core.DTOs
{
    /// <summary>
    /// DTO for creating a new role.
    /// </summary>
    public class RoleCreateDto
    {
        [Required]
        [StringLength(100)]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        public List<int> PermissionIDs { get; set; } = new List<int>();
    }
}