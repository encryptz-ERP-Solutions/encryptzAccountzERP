using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Entities.Core;
using Entities.Admin;

namespace Entities.Accounts
{
    [Table("ledger_entries", Schema = "public")]
    public class LedgerEntry
    {
        [Key]
        public Guid EntryID { get; set; }

        [Required]
        public Guid BusinessID { get; set; }

        [Required]
        public Guid VoucherID { get; set; }

        public Guid? LineID { get; set; }

        [Required]
        public DateTime EntryDate { get; set; }

        [Required]
        public Guid AccountID { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DebitAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditAmount { get; set; }

        [StringLength(3)]
        public string Currency { get; set; } = "INR";

        [Column(TypeName = "decimal(18,6)")]
        public decimal ExchangeRate { get; set; } = 1.000000m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseDebitAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseCreditAmount { get; set; }

        [StringLength(100)]
        public string? CostCenter { get; set; }

        [StringLength(100)]
        public string? Project { get; set; }

        [StringLength(20)]
        public string ReconciliationStatus { get; set; } = "unreconciled";

        public DateTime? ReconciledAt { get; set; }

        public Guid? ReconciledBy { get; set; }

        public bool IsOpeningBalance { get; set; }

        public string? Narration { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("BusinessID")]
        public virtual Business Business { get; set; }

        [ForeignKey("VoucherID")]
        public virtual Voucher Voucher { get; set; }

        [ForeignKey("LineID")]
        public virtual VoucherLine VoucherLine { get; set; }

        [ForeignKey("AccountID")]
        public virtual ChartOfAccount Account { get; set; }

        [ForeignKey("ReconciledBy")]
        public virtual User ReconciledByUser { get; set; }
    }
}
