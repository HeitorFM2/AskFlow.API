using AskFlow.Application.Common;
using FluentValidation;

namespace AskFlow.Application.Auth.Commands
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithErrorCode(ErrorCodes.RefreshTokenRequired).WithMessage("Refresh token is required.");
        }
    }
}
