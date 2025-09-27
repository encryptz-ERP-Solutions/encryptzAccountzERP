using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Core.DTOs
{
    /// <summary>
    /// DTO for updating an existing menu item.
    /// </summary>
    public class MenuItemUpdateDto
    {
        public int? ModuleID { get; set; }

        public int? ParentMenuItemID { get; set; }

        [StringLength(100)]
        public string? MenuText { get; set; }

        [StringLength(200)]
        public string? MenuURL { get; set; }

        [StringLength(50)]
        public string? IconClass { get; set; }

        public int? DisplayOrder { get; set; }

        public bool? IsActive { get; set; }
    }
}