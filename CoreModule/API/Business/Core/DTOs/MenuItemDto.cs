using System;

namespace BusinessLogic.Core.DTOs
{
    public class MenuItemDto
    {
        public int MenuItemID { get; set; }
        public int ModuleID { get; set; }
        public int? ParentMenuItemID { get; set; }
        public string MenuText { get; set; }
        public string? MenuURL { get; set; }
        public string? IconClass { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        // Audit fields
        public Guid? CreatedByUserID { get; set; }
        public DateTime? CreatedAtUTC { get; set; }
        public Guid? UpdatedByUserID { get; set; }
        public DateTime? UpdatedAtUTC { get; set; }
    }
}