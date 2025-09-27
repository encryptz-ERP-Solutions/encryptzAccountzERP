using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Core.DTOs
{
    /// <summary>
    /// DTO for creating a new module.
    /// </summary>
    public class ModuleCreateDto
    {
        [Required]
        [StringLength(100)]
        public string ModuleName { get; set; } = string.Empty;
    }
}