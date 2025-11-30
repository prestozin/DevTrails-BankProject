using DevTrails___BankProject.Enums;
using System.ComponentModel.DataAnnotations;

namespace DevTrails___BankProject.DTOs
{
    public class AccountCreateModel
    {
        [Required(ErrorMessage = "O CPF é obrigatório")]
        public string? CPF { get; set; }

        [Required(ErrorMessage = "Selecione o tipo da conta")]
        public AccountType AccountType { get; set; }
    }
}
