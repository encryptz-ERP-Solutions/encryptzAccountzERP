using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Entities.Core;
using Entities.Admin;

namespace Entities.Accounts
{
    [Table("ChartOfAccounts", Schema = "Acct")]
    public class ChartOfAccount
    {
        [Key]
        public Guid AccountID { get; set; }

        [Required]
        public Guid BusinessID { get; set; }

        [Required]
        public int AccountTypeID { get; set; }

        public Guid? ParentAccountID { get; set; }

        [Required]
        [StringLength(20)]
        public string AccountCode { get; set; }

        [Required]
        [StringLength(200)]
        public string AccountName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public bool IsSystemAccount { get; set; }

        [Required]
        public DateTime CreatedAtUTC { get; set; }

        public DateTime? UpdatedAtUTC { get; set; }

        [ForeignKey("BusinessID")]
        public virtual Business Business { get; set; }

        [ForeignKey("AccountTypeID")]
        public virtual AccountType AccountType { get; set; }

        [ForeignKey("ParentAccountID")]
        public virtual ChartOfAccount ParentAccount { get; set; }
    }
}
