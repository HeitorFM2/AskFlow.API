using FluentValidation;

namespace AskFlow.Application.Auth.Commands
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email é obrigatório.")
                .EmailAddress().WithMessage("Email inválido.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Senha é obrigatória.")
                .MinimumLength(6).WithMessage("Senha deve ter pelo menos 6 caracteres.");

            RuleFor(x => x.Identification)
                .NotEmpty().WithMessage("Identificação é obrigatória.")
                .MaximumLength(50).WithMessage("Identificação deve ter no máximo 50 caracteres.");
        }
    }
}
