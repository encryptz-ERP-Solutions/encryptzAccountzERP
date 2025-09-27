using System;

namespace BusinessLogic.Core.DTOs
{
    /// <summary>
    /// Represents a business data transfer object.
    /// </summary>
    public class BusinessDto
    {
        public Guid BusinessID { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string BusinessCode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? Gstin { get; set; }
        public string? TanNumber { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public int? StateID { get; set; }
        public string? PinCode { get; set; }
        public int? CountryID { get; set; }
        public DateTime CreatedAtUTC { get; set; }
        public DateTime UpdatedAtUTC { get; set; }
    }
}