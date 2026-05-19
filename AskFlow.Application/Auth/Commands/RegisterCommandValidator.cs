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
                .MinimumLength(8).WithErrorCode(ErrorCodes.PasswordMinLength).WithMessage("Password must be at least 8 characters long.")
                .Matches("[A-Z]").WithErrorCode(ErrorCodes.PasswordRequiresUppercase).WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithErrorCode(ErrorCodes.PasswordRequiresLowercase).WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithErrorCode(ErrorCodes.PasswordRequiresDigit).WithMessage("Password must contain at least one digit.");

            RuleFor(x => x.UserName)
                .NotEmpty().WithErrorCode(ErrorCodes.UserNameRequired).WithMessage("UserName is required.")
                .MaximumLength(50).WithErrorCode(ErrorCodes.UserNameMaxLength).WithMessage("UserName must be at most 50 characters.");

            RuleFor(x => x.Identification)
                .NotEmpty().WithErrorCode(ErrorCodes.IdentificationRequired).WithMessage("Identification is required.")
                .MaximumLength(100).WithErrorCode(ErrorCodes.IdentificationMaxLength).WithMessage("Identification must be at most 100 characters.");
        }
    }
}
