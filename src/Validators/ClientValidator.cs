using DevTrails___BankProject.DTOs;
using FluentValidation;

namespace DevTrails___BankProject.Validators
{
    public class ClientValidator : AbstractValidator<ClientCreateModel>
    {
        public ClientValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("O nome é obrigatório.")
                .MinimumLength(3).WithMessage("O nome deve ter no mínimo 3 caracteres.");

            RuleFor(c => c.CPF)
                .NotEmpty().WithMessage("O CPF é obrigatório.")
                .Length(11).WithMessage("O CPF deve conter exatamente 11 dígitos.")
                .Matches(@"^\d+$").WithMessage("O CPF deve conter apenas números."); //regex, permite apenas números

            RuleFor(c => c.BirthDate)
                .LessThan(DateTime.Now.AddYears(-16))
                .WithMessage("O cliente deve ser maior de 16 anos.");

            RuleFor(c => c.AccountType)
            .IsInEnum().WithMessage("Selecione um tipo de conta válido (Corrente ou Poupança).");
        }
    }
}
