using AskFlow.Application.Comments.Queries;
using AskFlow.Application.Comments.ViewModels;
using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using MediatR;

namespace AskFlow.Application.Comments.Handlers
{
    public class GetCommentRepliesHandler(ICommentRepository repository) : IRequestHandler<GetCommentRepliesQuery, Result<PagedResult<CommentViewModel>>>
    {
        public async Task<Result<PagedResult<CommentViewModel>>> Handle(GetCommentRepliesQuery request, CancellationToken cancellationToken)
        {
            var totalCount = await repository.CountRepliesAsync(request.CommentId, cancellationToken);
            var replies = await repository.GetRepliesAsync(request.CommentId, request.Page, request.PageSize, cancellationToken);

            return Result<PagedResult<CommentViewModel>>.Success(new PagedResult<CommentViewModel>
            {
                Items = replies,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
    }
}
