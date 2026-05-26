using AskFlow.Application.Common;
using FluentValidation;

namespace AskFlow.Application.Auth.Commands
{
    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithErrorCode(ErrorCodes.EmailRequired).WithMessage("Email is required.")
                .EmailAddress().WithErrorCode(ErrorCodes.EmailInvalid).WithMessage("Invalid email.");

            RuleFor(x => x.Token)
                .NotEmpty().WithErrorCode(ErrorCodes.ResetTokenRequired).WithMessage("Reset token is required.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithErrorCode(ErrorCodes.PasswordRequired).WithMessage("Password is required.")
                .MinimumLength(8).WithErrorCode(ErrorCodes.PasswordMinLength).WithMessage("Password must be at least 8 characters long.")
                .Matches("[A-Z]").WithErrorCode(ErrorCodes.PasswordRequiresUppercase).WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithErrorCode(ErrorCodes.PasswordRequiresLowercase).WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithErrorCode(ErrorCodes.PasswordRequiresDigit).WithMessage("Password must contain at least one digit.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithErrorCode(ErrorCodes.PasswordConfirmRequired).WithMessage("Password confirmation is required.")
                .Equal(x => x.NewPassword).WithErrorCode(ErrorCodes.PasswordConfirmMismatch).WithMessage("Passwords do not match.");
        }
    }
}
