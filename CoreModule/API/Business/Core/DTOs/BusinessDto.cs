using System;

namespace BusinessLogic.Core.DTOs
{
    public class BusinessDto
    {
        public Guid BusinessID { get; set; }
        public string BusinessName { get; set; }
        public string BusinessCode { get; set; } = string.Empty;
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public int? StateID { get; set; }
        public string? PinCode { get; set; }
        public int? CountryID { get; set; }
        public bool IsActive { get; set; }
        // Audit fields
        public Guid? CreatedByUserID { get; set; }
        public DateTime? CreatedAtUTC { get; set; }
        public Guid? UpdatedByUserID { get; set; }
        public DateTime? UpdatedAtUTC { get; set; }
    }
}