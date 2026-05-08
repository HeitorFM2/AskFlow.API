using FluentValidation;

namespace AskFlow.Application.Auth.Commands
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token é obrigatório.");
        }
    }
}
