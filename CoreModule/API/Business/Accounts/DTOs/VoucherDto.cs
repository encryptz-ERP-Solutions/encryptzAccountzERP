using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Accounts.DTOs
{
    public class VoucherDto
    {
        public Guid VoucherID { get; set; }
        public Guid BusinessID { get; set; }
        public string VoucherNumber { get; set; }
        public string VoucherType { get; set; }
        public DateTime VoucherDate { get; set; }
        public string? ReferenceNumber { get; set; }
        public DateTime? ReferenceDate { get; set; }
        public Guid? PartyAccountID { get; set; }
        public string? PartyName { get; set; }
        public string? PartyGstin { get; set; }
        public string? PlaceOfSupply { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal RoundOffAmount { get; set; }
        public decimal NetAmount { get; set; }
        public string Currency { get; set; }
        public decimal ExchangeRate { get; set; }
        public string Status { get; set; }
        public DateTime? PostedAt { get; set; }
        public Guid? PostedBy { get; set; }
        public string? Narration { get; set; }
        public bool IsReverseCharge { get; set; }
        public bool IsBillOfSupply { get; set; }
        public DateTime? DueDate { get; set; }
        public string? PaymentTerms { get; set; }
        public Guid? WarehouseID { get; set; }
        public string? CostCenter { get; set; }
        public string? Project { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
        public List<VoucherLineDto> Lines { get; set; } = new List<VoucherLineDto>();
    }

    public class CreateVoucherDto
    {
        [Required]
        public Guid BusinessID { get; set; }

        [Required]
        [StringLength(50)]
        public string VoucherType { get; set; }

        [Required]
        public DateTime VoucherDate { get; set; }

        public string? ReferenceNumber { get; set; }
        public DateTime? ReferenceDate { get; set; }
        public Guid? PartyAccountID { get; set; }
        public string? PartyName { get; set; }
        public string? PartyGstin { get; set; }
        public string? PlaceOfSupply { get; set; }
        public string? Narration { get; set; }
        public bool IsReverseCharge { get; set; }
        public bool IsBillOfSupply { get; set; }
        public DateTime? DueDate { get; set; }
        public string? PaymentTerms { get; set; }
        public Guid? WarehouseID { get; set; }
        public string? CostCenter { get; set; }
        public string? Project { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one voucher line is required")]
        public List<CreateVoucherLineDto> Lines { get; set; } = new List<CreateVoucherLineDto>();
    }

    public class UpdateVoucherDto
    {
        [Required]
        public DateTime VoucherDate { get; set; }

        public string? ReferenceNumber { get; set; }
        public DateTime? ReferenceDate { get; set; }
        public Guid? PartyAccountID { get; set; }
        public string? PartyName { get; set; }
        public string? PartyGstin { get; set; }
        public string? PlaceOfSupply { get; set; }
        public string? Narration { get; set; }
        public bool IsReverseCharge { get; set; }
        public bool IsBillOfSupply { get; set; }
        public DateTime? DueDate { get; set; }
        public string? PaymentTerms { get; set; }
        public Guid? WarehouseID { get; set; }
        public string? CostCenter { get; set; }
        public string? Project { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one voucher line is required")]
        public List<CreateVoucherLineDto> Lines { get; set; } = new List<CreateVoucherLineDto>();
    }

    public class VoucherLineDto
    {
        public Guid LineID { get; set; }
        public Guid VoucherID { get; set; }
        public int LineNumber { get; set; }
        public Guid AccountID { get; set; }
        public Guid? ItemID { get; set; }
        public string? Description { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxableAmount { get; set; }
        public Guid? TaxCodeID { get; set; }
        public decimal? TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal LineAmount { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public Guid? WarehouseID { get; set; }
        public string? CostCenter { get; set; }
        public string? Project { get; set; }
    }

    public class CreateVoucherLineDto
    {
        [Required]
        public Guid AccountID { get; set; }

        public Guid? ItemID { get; set; }
        public string? Description { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxableAmount { get; set; }
        public Guid? TaxCodeID { get; set; }
        public decimal? TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal CessAmount { get; set; }

        [Required]
        public decimal LineAmount { get; set; }

        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public Guid? WarehouseID { get; set; }
        public string? CostCenter { get; set; }
        public string? Project { get; set; }
    }

    public class PostVoucherResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Guid VoucherID { get; set; }
        public DateTime? PostedAt { get; set; }
    }
}
