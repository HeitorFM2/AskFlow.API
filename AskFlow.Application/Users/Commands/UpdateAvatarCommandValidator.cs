using AskFlow.Application.Common;
using FluentValidation;

namespace AskFlow.Application.Users.Commands
{
    public class UpdateAvatarCommandValidator : AbstractValidator<UpdateAvatarCommand>
    {
        private const long MaxBytes = 2 * 1024 * 1024;

        public UpdateAvatarCommandValidator()
        {
            RuleFor(x => x.Content)
                .NotNull().WithErrorCode(ErrorCodes.AvatarRequired).WithMessage("Avatar file is required.");

            RuleFor(x => x.Length)
                .GreaterThan(0).WithErrorCode(ErrorCodes.AvatarRequired).WithMessage("Avatar file is required.")
                .LessThanOrEqualTo(MaxBytes).WithErrorCode(ErrorCodes.AvatarTooLarge).WithMessage("Avatar must be at most 2 MB.");
        }
    }
}
