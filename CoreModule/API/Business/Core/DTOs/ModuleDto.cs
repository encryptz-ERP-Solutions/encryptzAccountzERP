using System;

namespace BusinessLogic.Core.DTOs
{
    public class ModuleDto
    {
        public int ModuleID { get; set; }
        public string ModuleName { get; set; }
        public bool IsSystemModule { get; set; }
        public bool IsActive { get; set; }
        // Audit fields
        public Guid? CreatedByUserID { get; set; }
        public DateTime? CreatedAtUTC { get; set; }
        public Guid? UpdatedByUserID { get; set; }
        public DateTime? UpdatedAtUTC { get; set; }
    }
}