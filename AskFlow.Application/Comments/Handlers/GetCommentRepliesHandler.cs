using AskFlow.Application.Comments.Queries;
using AskFlow.Application.Comments.ViewModels;
using AskFlow.Application.Common;
using AskFlow.Application.Users.ViewModels;
using AskFlow.Domain.Interfaces;
using MediatR;

namespace AskFlow.Application.Comments.Handlers
{
    public class GetCommentRepliesHandler(ICommentRepository repository) : IRequestHandler<GetCommentRepliesQuery, Result<PagedResult<CommentViewModel>>>
    {
        public async Task<Result<PagedResult<CommentViewModel>>> Handle(GetCommentRepliesQuery request, CancellationToken cancellationToken)
        {
            var totalCount = await repository.CountRepliesAsync(request.CommentId, cancellationToken);
            var replies = await repository.GetRepliesAsync(request.CommentId, request.Page, request.PageSize, cancellationToken);

            var items = replies.Select(c => new CommentViewModel
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                ParentCommentId = c.ParentCommentId,
                ReplyCount = c.Replies.Count,
                User = new UserViewModel
                {
                    UserName = c.User.UserName ?? string.Empty,
                    Identification = c.User.Identification
                }
            }).ToList();

            return Result<PagedResult<CommentViewModel>>.Success(new PagedResult<CommentViewModel>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
    }
}
