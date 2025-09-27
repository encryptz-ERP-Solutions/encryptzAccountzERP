using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Core.DTOs
{
    /// <summary>
    /// DTO for updating an existing module.
    /// </summary>
    public class ModuleUpdateDto
    {
        [StringLength(100)]
        public string? ModuleName { get; set; }

        public bool? IsActive { get; set; }
    }
}