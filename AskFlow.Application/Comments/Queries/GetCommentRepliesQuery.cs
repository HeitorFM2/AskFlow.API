using AskFlow.Application.Comments.ViewModels;
using AskFlow.Application.Common;
using MediatR;

namespace AskFlow.Application.Comments.Queries
{
    public record GetCommentRepliesQuery(int CommentId, int Page = 1, int PageSize = 20) : IRequest<Result<PagedResult<CommentViewModel>>>;
}
