using AskFlow.Application.Common;
using FluentValidation;

namespace AskFlow.Application.Posts.Command
{
    public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
    {
        public CreatePostCommandValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithErrorCode(ErrorCodes.PostContentRequired).WithMessage("Content is required.")
                .MaximumLength(280).WithErrorCode(ErrorCodes.PostContentMaxLength).WithMessage("Content must be at most 280 characters.");
        }
    }
}
