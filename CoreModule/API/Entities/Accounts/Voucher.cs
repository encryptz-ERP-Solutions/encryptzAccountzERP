using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Entities.Core;
using Entities.Admin;

namespace Entities.Accounts
{
    [Table("vouchers", Schema = "public")]
    public class Voucher
    {
        [Key]
        public Guid VoucherID { get; set; }

        [Required]
        public Guid BusinessID { get; set; }

        [Required]
        [StringLength(100)]
        public string VoucherNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string VoucherType { get; set; }

        [Required]
        public DateTime VoucherDate { get; set; }

        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        public DateTime? ReferenceDate { get; set; }

        public Guid? PartyAccountID { get; set; }

        [StringLength(255)]
        public string? PartyName { get; set; }

        [StringLength(15)]
        public string? PartyGstin { get; set; }

        [StringLength(100)]
        public string? PlaceOfSupply { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RoundOffAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetAmount { get; set; }

        [StringLength(3)]
        public string Currency { get; set; } = "INR";

        [Column(TypeName = "decimal(18,6)")]
        public decimal ExchangeRate { get; set; } = 1.000000m;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "draft";

        public DateTime? PostedAt { get; set; }

        public Guid? PostedBy { get; set; }

        public string? Narration { get; set; }

        public bool IsReverseCharge { get; set; }

        public bool IsBillOfSupply { get; set; }

        public DateTime? DueDate { get; set; }

        [StringLength(100)]
        public string? PaymentTerms { get; set; }

        public Guid? WarehouseID { get; set; }

        [StringLength(100)]
        public string? CostCenter { get; set; }

        [StringLength(100)]
        public string? Project { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        public Guid? CreatedBy { get; set; }

        public Guid? UpdatedBy { get; set; }

        // Navigation properties
        [ForeignKey("BusinessID")]
        public virtual Business Business { get; set; }

        [ForeignKey("PartyAccountID")]
        public virtual ChartOfAccount PartyAccount { get; set; }

        [ForeignKey("PostedBy")]
        public virtual User PostedByUser { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User CreatedByUser { get; set; }

        [ForeignKey("UpdatedBy")]
        public virtual User UpdatedByUser { get; set; }

        public virtual ICollection<VoucherLine> VoucherLines { get; set; } = new List<VoucherLine>();
    }
}
