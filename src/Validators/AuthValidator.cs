using DevTrails___BankProject.DTOs.InputModel;
using FluentValidation;

namespace DevTrails___BankProject.Validators
{
    public class AuthValidator
    {
        public class RegisterValidator : AbstractValidator<RegisterInputModel>
        {
            public RegisterValidator()
            {
                RuleFor(x => x.Email)
                    .NotEmpty().WithMessage("Email é obrigatório.")
                    .EmailAddress().WithMessage("Formato de email inválido.");

                RuleFor(x => x.Password)
                    .NotEmpty().WithMessage("Senha é obrigatória.")
                    .MinimumLength(6).WithMessage("A senha deve ter pelo menos 6 caracteres.")
   
                    .Matches(@"[A-Z]").WithMessage("A senha deve conter pelo menos uma letra maiúscula.")
                    .Matches(@"[0-9]").WithMessage("A senha deve conter pelo menos um número.");

                RuleFor(x => x.ConfirmPassword)
                    .Equal(x => x.Password).WithMessage("As senhas não conferem.");
            }
        }

        public class LoginValidator : AbstractValidator<LoginInputModel>
        {
            public LoginValidator()
            {
                RuleFor(x => x.Email)
                    .NotEmpty().WithMessage("Email é obrigatório.")
                    .EmailAddress().WithMessage("Email inválido.");

                RuleFor(x => x.Password)
                    .NotEmpty().WithMessage("Senha é obrigatória.");
            }
        }
    }
}
