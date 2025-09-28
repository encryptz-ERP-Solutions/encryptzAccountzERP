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
    }
}