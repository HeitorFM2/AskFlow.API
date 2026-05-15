using AskFlow.Application.Common;
using FluentValidation;

namespace AskFlow.Application.Comments.Queries
{
    public class GetCommentRepliesQueryValidator : AbstractValidator<GetCommentRepliesQuery>
    {
        public GetCommentRepliesQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1)
                .WithErrorCode(ErrorCodes.PaginationPageInvalid)
                .WithMessage("Page must be greater than or equal to 1.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithErrorCode(ErrorCodes.PaginationPageSizeInvalid)
                .WithMessage("PageSize must be between 1 and 100.");
        }
    }
}
