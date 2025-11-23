using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Accounts
{
    [Table("voucher_lines", Schema = "public")]
    public class VoucherLine
    {
        [Key]
        public Guid LineID { get; set; }

        [Required]
        public Guid VoucherID { get; set; }

        [Required]
        public int LineNumber { get; set; }

        [Required]
        public Guid AccountID { get; set; }

        public Guid? ItemID { get; set; }

        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? DiscountPercentage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxableAmount { get; set; }

        public Guid? TaxCodeID { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? TaxRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CgstAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SgstAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal IgstAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CessAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal LineAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DebitAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditAmount { get; set; }

        public Guid? WarehouseID { get; set; }

        [StringLength(100)]
        public string? CostCenter { get; set; }

        [StringLength(100)]
        public string? Project { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("VoucherID")]
        public virtual Voucher Voucher { get; set; }

        [ForeignKey("AccountID")]
        public virtual ChartOfAccount Account { get; set; }
    }
}
