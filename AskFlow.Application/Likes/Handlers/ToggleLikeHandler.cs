using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Likes.Commands;
using MediatR;

namespace AskFlow.Application.Likes.Handlers
{
    public class ToggleLikeHandler(
        ILikeRepository repository,
        ICurrentUserService currentUserService) : IRequestHandler<ToggleLikeCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(ToggleLikeCommand request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Result<bool>.Unauthorized(
                    ErrorCodes.UserNotAuthenticated, "User not authenticated.");

            var likedPost = await repository.ToggleLikeAsync(userId, request.PostId, cancellationToken);

            return Result<bool>.Success(likedPost);
        }
    }
}
