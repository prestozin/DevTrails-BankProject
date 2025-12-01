using DevTrails___BankProject.Enums;

namespace DevTrails___BankProject.DTOs
{
    public class ClientCreateModel
    {
        public string? Name { get; set; }
        public string? CPF { get; set; }
        public DateTime BirthDate { get; set; }
        public AccountType AccountType { get; set; }
    }
}
