using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Core.DTOs
{
    /// <summary>
    /// DTO for updating an existing business. All properties are optional.
    /// </summary>
    public class BusinessUpdateDto
    {
        [StringLength(100)]
        public string? BusinessName { get; set; }

        [StringLength(20)]
        public string? BusinessCode { get; set; }

        public bool? IsActive { get; set; }

        [StringLength(15)]
        public string? Gstin { get; set; }

        [StringLength(10)]
        public string? TanNumber { get; set; }

        [StringLength(200)]
        public string? AddressLine1 { get; set; }

        [StringLength(200)]
        public string? AddressLine2 { get; set; }

        [StringLength(50)]
        public string? City { get; set; }

        public int? StateID { get; set; }

        [StringLength(10)]
        public string? PinCode { get; set; }

        public int? CountryID { get; set; }
    }
}