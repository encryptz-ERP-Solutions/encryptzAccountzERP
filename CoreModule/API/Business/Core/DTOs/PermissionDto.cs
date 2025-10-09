namespace BusinessLogic.Core.DTOs
{
    public class PermissionDto
    {
        public int PermissionID { get; set; }
        public string PermissionKey { get; set; }
        public string Description { get; set; }
        public int? MenuItemID { get; set; }
        public int ModuleID { get; set; }
    }
}
