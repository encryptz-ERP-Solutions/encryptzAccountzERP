using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Core.DTOs
{
    /// <summary>
    /// DTO for assigning a role to a user within a business.
    /// </summary>
    public class AssignRoleDto
    {
        [Required]
        public Guid UserID { get; set; }

        [Required]
        public Guid BusinessID { get; set; }

        [Required]
        public int RoleID { get; set; }
    }
}