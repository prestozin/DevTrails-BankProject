using DevTrails___BankProject.Entities;

namespace DevTrails___BankProject.DTOs
{
    public class AccountViewModel
    {
        public Guid AccountId { get; set; }
        public string? Number { get; set; }
        public string? AccountType { get; set; }
        public decimal Balance { get; set; }
        public string? Status { get; set; }

        public string? ClientName { get; set; }
        public Guid ClientId { get; set; }

        public static AccountViewModel FromModel(Account account)
        {
            string accountType = account is SavingsAccount ? "Savings" : "Checking";

            return new AccountViewModel
            {
                AccountId = account.AccountID,
                Number = account.Number,
                Balance = account.Balance,
                AccountType = accountType,
                Status = account.AccountStatus.ToString(),
                ClientName = account.Client?.Name,
                ClientId = account.ClientId
            }; 
        }
    }
}
