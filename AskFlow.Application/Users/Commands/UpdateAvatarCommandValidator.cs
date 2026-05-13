using AskFlow.Application.Common;
using FluentValidation;

namespace AskFlow.Application.Users.Commands
{
    public class UpdateAvatarCommandValidator : AbstractValidator<UpdateAvatarCommand>
    {
        private const long MaxBytes = 2 * 1024 * 1024;
        private static readonly string[] AllowedTypes = ["image/jpeg", "image/png", "image/webp"];

        public UpdateAvatarCommandValidator()
        {
            RuleFor(x => x.Content)
                .NotNull().WithErrorCode(ErrorCodes.AvatarRequired).WithMessage("Avatar file is required.");

            RuleFor(x => x.Length)
                .GreaterThan(0).WithErrorCode(ErrorCodes.AvatarRequired).WithMessage("Avatar file is required.")
                .LessThanOrEqualTo(MaxBytes).WithErrorCode(ErrorCodes.AvatarTooLarge).WithMessage("Avatar must be at most 2 MB.");

            RuleFor(x => x.ContentType)
                .Must(t => AllowedTypes.Contains(t)).WithErrorCode(ErrorCodes.AvatarContentTypeInvalid)
                .WithMessage("Avatar must be image/jpeg, image/png or image/webp.");
        }
    }
}
