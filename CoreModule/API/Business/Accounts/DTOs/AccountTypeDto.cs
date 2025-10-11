namespace BusinessLogic.Accounts.DTOs
{
    public class AccountTypeDto
    {
        public int AccountTypeID { get; set; }
        public string AccountTypeName { get; set; }
        public string NormalBalance { get; set; }
    }

    public class CreateAccountTypeDto
    {
        public string AccountTypeName { get; set; }
        public string NormalBalance { get; set; }
    }

    public class UpdateAccountTypeDto
    {
        public string AccountTypeName { get; set; }
        public string NormalBalance { get; set; }
    }
}
