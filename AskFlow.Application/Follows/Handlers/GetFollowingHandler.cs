using AskFlow.Application.Common;
using AskFlow.Application.Follows.Queries;
using AskFlow.Application.Follows.ViewModels;
using AskFlow.Application.Interfaces;
using MediatR;

namespace AskFlow.Application.Follows.Handlers
{
    public class GetFollowingHandler(
        IFollowQueries queries,
        ICurrentUserService currentUserService) : IRequestHandler<GetFollowingQuery, Result<PagedResult<FollowViewModel>>>
    {
        public async Task<Result<PagedResult<FollowViewModel>>> Handle(GetFollowingQuery query, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Result<PagedResult<FollowViewModel>>.Unauthorized(
                    ErrorCodes.UserNotAuthenticated, "User not authenticated.");

            var items = await queries.GetFollowingAsync(userId, query.Page, query.PageSize, query.Search, cancellationToken);
            var total = await queries.CountFollowingAsync(userId, query.Search, cancellationToken);

            return Result<PagedResult<FollowViewModel>>.Success(new PagedResult<FollowViewModel>
            {
                Items = items,
                TotalCount = total,
                Page = query.Page,
                PageSize = query.PageSize
            });
        }
    }
}
