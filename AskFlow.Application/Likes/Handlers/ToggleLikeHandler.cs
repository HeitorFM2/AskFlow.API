using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Likes.Commands;
using MediatR;

namespace AskFlow.Application.Likes.Handlers
{
    public class ToggleLikeHandler(
        ILikeRepository likeRepository,
        IPostRepository postRepository,
        ICurrentUserService currentUserService) : IRequestHandler<ToggleLikeCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(ToggleLikeCommand request, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Result<bool>.Unauthorized(ErrorCodes.UserNotAuthenticated, "User not authenticated.");

            var post = await postRepository.FindByIdAsync(request.PostId);
            if (post is null)
                return Result<bool>.NotFound(ErrorCodes.PostNotFound, "Post not found.");

            var liked = await likeRepository.ToggleLikeAsync(userId, request.PostId, cancellationToken);
            return Result<bool>.Success(liked);
        }
    }
}
