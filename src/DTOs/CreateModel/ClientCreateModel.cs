using DevTrails___BankProject.Enums;
using System.ComponentModel.DataAnnotations;

namespace DevTrails___BankProject.DTOs
{
    public class ClientCreateModel
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "O CPF é obrigatório")]
        public string? CPF { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        public AccountType AccountType { get; set; }
    }
}
