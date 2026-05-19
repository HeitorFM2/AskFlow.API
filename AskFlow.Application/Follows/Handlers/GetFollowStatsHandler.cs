using AskFlow.Application.Common;
using AskFlow.Application.Follows.Queries;
using AskFlow.Application.Follows.ViewModels;
using AskFlow.Application.Interfaces;
using MediatR;

namespace AskFlow.Application.Follows.Handlers
{
    public class GetFollowStatsHandler(
        IFollowQueries queries,
        ICurrentUserService currentUserService) : IRequestHandler<GetFollowStatsQuery, Result<FollowStatsViewModel>>
    {
        public async Task<Result<FollowStatsViewModel>> Handle(GetFollowStatsQuery query, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Result<FollowStatsViewModel>.Unauthorized(
                    ErrorCodes.UserNotAuthenticated, "User not authenticated.");

            var followers = await queries.CountFollowersAsync(userId, cancellationToken: cancellationToken);
            var following = await queries.CountFollowingAsync(userId, cancellationToken: cancellationToken);

            return Result<FollowStatsViewModel>.Success(new FollowStatsViewModel
            {
                Followers = followers,
                Following = following
            });
        }
    }
}
