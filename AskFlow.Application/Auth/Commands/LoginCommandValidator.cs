using AskFlow.Application.Common;
using FluentValidation;

namespace AskFlow.Application.Auth.Commands
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithErrorCode(ErrorCodes.EmailRequired).WithMessage("Email is required.")
                .EmailAddress().WithErrorCode(ErrorCodes.EmailInvalid).WithMessage("Invalid email.");

            RuleFor(x => x.Password)
                .NotEmpty().WithErrorCode(ErrorCodes.PasswordRequired).WithMessage("Password is required.");
        }
    }
}
