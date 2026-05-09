using AskFlow.Application.Common;
using FluentValidation;

namespace AskFlow.Application.Auth.Commands
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithErrorCode(ErrorCodes.EmailRequired).WithMessage("Email is required.")
                .EmailAddress().WithErrorCode(ErrorCodes.EmailInvalid).WithMessage("Invalid email.");

            RuleFor(x => x.Password)
                .NotEmpty().WithErrorCode(ErrorCodes.PasswordRequired).WithMessage("Password is required.")
                .MinimumLength(6).WithErrorCode(ErrorCodes.PasswordMinLength).WithMessage("Password must be at least 6 characters long.");

            RuleFor(x => x.Identification)
                .NotEmpty().WithErrorCode(ErrorCodes.IdentificationRequired).WithMessage("Identification is required.")
                .MaximumLength(50).WithErrorCode(ErrorCodes.IdentificationMaxLength).WithMessage("Identification must be at most 50 characters.");
        }
    }
}
