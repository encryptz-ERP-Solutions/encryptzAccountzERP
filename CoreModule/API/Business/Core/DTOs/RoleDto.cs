namespace BusinessLogic.Core.DTOs
{
    public class RoleDto
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public string? Description { get; set; }
        public bool IsSystemRole { get; set; }
    }
}
