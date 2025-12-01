using DevTrails___BankProject.Enums;

namespace DevTrails___BankProject.DTOs
{
    public class AccountCreateModel
    {
        public string? CPF { get; set; }
        public AccountType AccountType { get; set; }
    }
}
