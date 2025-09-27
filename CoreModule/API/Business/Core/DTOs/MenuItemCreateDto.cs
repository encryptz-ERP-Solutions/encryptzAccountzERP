using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Core.DTOs
{
    /// <summary>
    /// DTO for creating a new menu item.
    /// </summary>
    public class MenuItemCreateDto
    {
        [Required]
        public int ModuleID { get; set; }

        public int? ParentMenuItemID { get; set; }

        [Required]
        [StringLength(100)]
        public string MenuText { get; set; } = string.Empty;

        [StringLength(200)]
        public string? MenuURL { get; set; }

        [StringLength(50)]
        public string? IconClass { get; set; }

        [Required]
        public int DisplayOrder { get; set; }
    }
}