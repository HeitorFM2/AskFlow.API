using AskFlow.Application.Comments.Queries;
using AskFlow.Application.Comments.ViewModels;
using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using MediatR;

namespace AskFlow.Application.Comments.Handlers
{
    public class GetCommentsByPostHandler(ICommentQueries queries) : IRequestHandler<GetCommentsByPostQuery, Result<PagedResult<CommentViewModel>>>
    {
        public async Task<Result<PagedResult<CommentViewModel>>> Handle(GetCommentsByPostQuery request, CancellationToken cancellationToken)
        {
            var totalCount = await queries.CountByPostAsync(request.PostId, cancellationToken);
            var comments = await queries.GetByPostAsync(request.PostId, request.Page, request.PageSize, cancellationToken);

            return Result<PagedResult<CommentViewModel>>.Success(new PagedResult<CommentViewModel>
            {
                Items = comments,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
    }
}
