using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Accounts
{
    [Table("AccountTypes", Schema = "Acct")]
    public class AccountType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AccountTypeID { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountTypeName { get; set; }

        [Required]
        [StringLength(2)]
        public string NormalBalance { get; set; }
    }
}
