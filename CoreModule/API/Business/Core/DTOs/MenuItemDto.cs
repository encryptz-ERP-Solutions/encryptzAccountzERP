using System;

namespace BusinessLogic.Core.DTOs
{
    /// <summary>
    /// Represents a menu item data transfer object.
    /// </summary>
    public class MenuItemDto
    {
        public int MenuItemID { get; set; }
        public int ModuleID { get; set; }
        public int? ParentMenuItemID { get; set; }
        public string MenuText { get; set; } = string.Empty;
        public string? MenuURL { get; set; }
        public string? IconClass { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}