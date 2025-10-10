using System;

namespace BusinessLogic.Accounts.DTOs
{
    public class ChartOfAccountDto
    {
        public Guid AccountID { get; set; }
        public Guid BusinessID { get; set; }
        public int AccountTypeID { get; set; }
        public Guid? ParentAccountID { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsSystemAccount { get; set; }
        public DateTime CreatedAtUTC { get; set; }
        public DateTime? UpdatedAtUTC { get; set; }
    }

    public class CreateChartOfAccountDto
    {
        public Guid BusinessID { get; set; }
        public int AccountTypeID { get; set; }
        public Guid? ParentAccountID { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateChartOfAccountDto
    {
        public string AccountName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
