using AskFlow.Application.Common;
using FluentValidation;

namespace AskFlow.Application.Users.Commands
{
    public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
    {
        public UpdateProfileCommandValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithErrorCode(ErrorCodes.UserNameRequired).WithMessage("UserName is required.")
                .MaximumLength(50).WithErrorCode(ErrorCodes.UserNameMaxLength).WithMessage("UserName must be at most 50 characters.");

            RuleFor(x => x.Identification)
                .NotEmpty().WithErrorCode(ErrorCodes.IdentificationRequired).WithMessage("Identification is required.")
                .MaximumLength(50).WithErrorCode(ErrorCodes.IdentificationMaxLength).WithMessage("Identification must be at most 50 characters.");
        }
    }
}
