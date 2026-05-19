using AskFlow.Application.Common;
using AskFlow.Application.Follows.Queries;
using AskFlow.Application.Follows.ViewModels;
using AskFlow.Application.Interfaces;
using MediatR;

namespace AskFlow.Application.Follows.Handlers
{
    public class GetFollowersHandler(
        IFollowQueries queries,
        ICurrentUserService currentUserService) : IRequestHandler<GetFollowersQuery, Result<PagedResult<FollowViewModel>>>
    {
        public async Task<Result<PagedResult<FollowViewModel>>> Handle(GetFollowersQuery query, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Result<PagedResult<FollowViewModel>>.Unauthorized(
                    ErrorCodes.UserNotAuthenticated, "User not authenticated.");

            var items = await queries.GetFollowersAsync(userId, query.Page, query.PageSize, query.Search, cancellationToken);
            var total = await queries.CountFollowersAsync(userId, query.Search, cancellationToken);

            if (items.Count > 0)
            {
                var followedNames = await queries.GetFollowedUserNamesAsync(userId, items.Select(f => f.UserName), cancellationToken);
                foreach (var item in items)
                    item.IsFollowing = followedNames.Contains(item.UserName);
            }

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
