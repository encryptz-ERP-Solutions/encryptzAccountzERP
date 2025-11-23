using System;

namespace BusinessLogic.Core.DTOs
{
    public class PermissionDto
    {
        public int PermissionID { get; set; }
        public string PermissionKey { get; set; }
        public string Description { get; set; }
        public int? MenuItemID { get; set; }
        public int ModuleID { get; set; }
        // Audit fields
        public Guid? CreatedByUserID { get; set; }
        public DateTime? CreatedAtUTC { get; set; }
        public Guid? UpdatedByUserID { get; set; }
        public DateTime? UpdatedAtUTC { get; set; }
    }
}
