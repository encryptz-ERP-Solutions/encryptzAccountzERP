namespace BusinessLogic.Core.DTOs
{
    /// <summary>
    /// Represents a permission data transfer object.
    /// </summary>
    public class PermissionDto
    {
        public int PermissionID { get; set; }
        public string PermissionKey { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ModuleID { get; set; }
    }
}