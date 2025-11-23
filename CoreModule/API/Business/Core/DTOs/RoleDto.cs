using System;

namespace BusinessLogic.Core.DTOs
{
    public class RoleDto
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public string? Description { get; set; }
        public bool IsSystemRole { get; set; }
        // Audit fields
        public Guid? CreatedByUserID { get; set; }
        public DateTime? CreatedAtUTC { get; set; }
        public Guid? UpdatedByUserID { get; set; }
        public DateTime? UpdatedAtUTC { get; set; }
    }
}
