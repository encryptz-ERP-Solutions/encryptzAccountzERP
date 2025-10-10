using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Accounts
{
    [Table("TransactionDetails", Schema = "Acct")]
    public class TransactionDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long TransactionDetailID { get; set; }

        [Required]
        public Guid TransactionHeaderID { get; set; }

        [Required]
        public Guid AccountID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DebitAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal CreditAmount { get; set; }

        [ForeignKey("TransactionHeaderID")]
        public virtual TransactionHeader TransactionHeader { get; set; }

        [ForeignKey("AccountID")]
        public virtual ChartOfAccount ChartOfAccount { get; set; }
    }
}
