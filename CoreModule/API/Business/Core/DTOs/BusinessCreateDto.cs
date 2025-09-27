using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Core.DTOs
{
    /// <summary>
    /// DTO for creating a new business.
    /// </summary>
    public class BusinessCreateDto
    {
        [Required]
        [StringLength(100)]
        public string BusinessName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string BusinessCode { get; set; } = string.Empty;

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