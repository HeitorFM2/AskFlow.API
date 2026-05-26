using AskFlow.Application.Common;
using FluentValidation;

namespace AskFlow.Application.Auth.Commands
{
    public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithErrorCode(ErrorCodes.EmailRequired).WithMessage("Email is required.")
                .EmailAddress().WithErrorCode(ErrorCodes.EmailInvalid).WithMessage("Invalid email.");
        }
    }
}
