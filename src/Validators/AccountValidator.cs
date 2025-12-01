using FluentValidation;
using DevTrails___BankProject.DTOs;

namespace DevTrails___BankProject.Validators
{
    public class AccountValidator : AbstractValidator<AccountCreateModel>
    {
        public AccountValidator()
        {
            RuleFor(x => x.CPF)
                .NotEmpty().WithMessage("O CPF é obrigatório.")
                .Length(11).WithMessage("O CPF deve conter exatamente 11 números.")
                .Matches(@"^\d+$").WithMessage("O CPF deve conter apenas números.");

            RuleFor(x => x.AccountType)
                .IsInEnum().WithMessage("Tipo de conta inválido. Informe 1 (Corrente) ou 2 (Poupança).");


        }
    }
}