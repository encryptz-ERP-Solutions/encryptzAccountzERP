namespace BusinessLogic.Core.DTOs
{
    public class ModuleDto
    {
        public int ModuleID { get; set; }
        public string ModuleName { get; set; }
        public bool IsSystemModule { get; set; }
        public bool IsActive { get; set; }
    }
}