using DevTrails___BankProject.DTOs;
using FluentValidation;

namespace DevTrails___BankProject.Validators
{
    public class DepositValidator : AbstractValidator<DepositInputModel>
    {

        public DepositValidator()
        {
            RuleFor(x => x.AccountNumber)
                .NotEmpty().WithMessage("Número da conta obrigatório.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("O valor do depósito deve ser maior que zero.");
        }
    }

    public class WithdrawValidator : AbstractValidator<WithdrawInputModel>
    {
        public WithdrawValidator()
        {
            RuleFor(x => x.AccountNumber)
                .NotEmpty().WithMessage("Número da conta obrigatório.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("O valor do saque deve ser maior que zero.");
        }
    }

    public class TransferValidator : AbstractValidator<TransferInputModel> 
    {
        public TransferValidator()
        {
            RuleFor(x => x.FromAccountNumber)
               .NotEmpty().WithMessage("Número da conta de origem é obrigatório.");

            RuleFor(x => x.ToAccountNumber)
                .NotEmpty().WithMessage("Número da conta de destino é obrigatório.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("O valor da transferência deve ser maior que zero.");

            RuleFor(x => x)
                .Must(x => x.FromAccountNumber != x.ToAccountNumber)
                .WithMessage("A conta de origem e destino não podem ser iguais.");
        }
    }
}
