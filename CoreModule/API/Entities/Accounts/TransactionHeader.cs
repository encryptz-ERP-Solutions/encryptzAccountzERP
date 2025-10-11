using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Entities.Core;
using Entities.Admin;

namespace Entities.Accounts
{
    [Table("TransactionHeaders", Schema = "Acct")]
    public class TransactionHeader
    {
        [Key]
        public Guid TransactionHeaderID { get; set; }

        [Required]
        public Guid BusinessID { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; }

        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public Guid CreatedByUserID { get; set; }

        [Required]
        public DateTime CreatedAtUTC { get; set; }

        [ForeignKey("BusinessID")]
        public virtual Business Business { get; set; }

        [ForeignKey("CreatedByUserID")]
        public virtual User User { get; set; }

        public virtual ICollection<TransactionDetail> TransactionDetails { get; set; }
    }
}
