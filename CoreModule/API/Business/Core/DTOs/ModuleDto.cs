namespace BusinessLogic.Core.DTOs
{
    /// <summary>
    /// Represents a module data transfer object.
    /// </summary>
    public class ModuleDto
    {
        public int ModuleID { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public bool IsSystemModule { get; set; }
        public bool IsActive { get; set; }
    }
}