using AskFlow.Application.Common;
using AskFlow.Application.Follows.Queries;
using AskFlow.Application.Interfaces;
using MediatR;

namespace AskFlow.Application.Follows.Handlers
{
    public class GetIsFollowingHandler(
        IFollowQueries queries,
        ICurrentUserService currentUserService) : IRequestHandler<GetIsFollowingQuery, Result<bool>>
    {
        public async Task<Result<bool>> Handle(GetIsFollowingQuery query, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Result<bool>.Unauthorized(
                    ErrorCodes.UserNotAuthenticated, "User not authenticated.");

            var isFollowing = await queries.IsFollowingAsync(userId, query.TargetUserName, cancellationToken);

            return Result<bool>.Success(isFollowing);
        }
    }
}
