using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Users.Queries;
using AskFlow.Application.Users.ViewModels;
using MediatR;

namespace AskFlow.Application.Users.Handlers
{
    public class GetAllUsersHandler(
        IUserQueries userQueries,
        ICurrentUserService currentUserService) : IRequestHandler<GetAllUsersQuery, Result<IReadOnlyList<UserDto>>>
    {
        public async Task<Result<IReadOnlyList<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Result<IReadOnlyList<UserDto>>.Unauthorized(ErrorCodes.UserNotAuthenticated, "User not authenticated.");

            var users = await userQueries.GetAllAsync(userId, request.Search, cancellationToken);

            return Result<IReadOnlyList<UserDto>>.Success(users);
        }
    }
}
