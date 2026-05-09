using AskFlow.Application.Common;
using FluentValidation;

namespace AskFlow.Application.Comments.Commands
{
    public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
    {
        public CreateCommentCommandValidator()
        {
            RuleFor(x => x.PostId)
                .GreaterThan(0).WithErrorCode(ErrorCodes.CommentPostIdInvalid).WithMessage("Invalid post.");

            RuleFor(x => x.Content)
                .NotEmpty().WithErrorCode(ErrorCodes.CommentContentRequired).WithMessage("Content is required.")
                .MaximumLength(500).WithErrorCode(ErrorCodes.CommentContentMaxLength).WithMessage("Content must be at most 500 characters.");
        }
    }
}
