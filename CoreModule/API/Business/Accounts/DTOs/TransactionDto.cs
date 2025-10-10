using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Accounts.DTOs
{
    // DTO for reading a single transaction detail line
    public class TransactionDetailDto
    {
        public long TransactionDetailID { get; set; }
        public Guid AccountID { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
    }

    // DTO for reading a transaction header with its details
    public class TransactionHeaderDto
    {
        public Guid TransactionHeaderID { get; set; }
        public Guid BusinessID { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? ReferenceNumber { get; set; }
        public string Description { get; set; }
        public Guid CreatedByUserID { get; set; }
        public DateTime CreatedAtUTC { get; set; }
        public ICollection<TransactionDetailDto> TransactionDetails { get; set; }
    }

    // DTO for creating a new transaction detail line
    public class CreateTransactionDetailDto
    {
        [Required]
        public Guid AccountID { get; set; }
        public decimal? DebitAmount { get; set; }
        public decimal? CreditAmount { get; set; }
    }

    // Composite DTO for creating a new transaction (header and details together)
    public class CreateTransactionDto
    {
        [Required]
        public Guid BusinessID { get; set; }
        [Required]
        public DateTime TransactionDate { get; set; }
        public string? ReferenceNumber { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public Guid CreatedByUserID { get; set; }
        [Required]
        public ICollection<CreateTransactionDetailDto> TransactionDetails { get; set; }
    }

    // DTO for updating the header of an existing transaction
    public class UpdateTransactionHeaderDto
    {
        public string? ReferenceNumber { get; set; }
        [Required]
        public string Description { get; set; }
    }
}
