using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Posts.Command;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AskFlow.Application.Posts.Handlers
{
    public class DeletePostHandler(
        IPostRepository repository,
        ICurrentUserService currentUserService,
        ILogger<DeletePostHandler> logger) : IRequestHandler<DeletePostCommand, Result>
    {
        public async Task<Result> Handle(DeletePostCommand command, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Result.Unauthorized(ErrorCodes.UserNotAuthenticated, "User not authenticated.");

            var post = await repository.FindByIdAsync(command.PostId);

            if (post is null)
                return Result.NotFound(ErrorCodes.PostNotFound, "Post not found.");

            if (post.UserId != userId)
            {
                logger.LogWarning("User {UserId} attempted to delete post {PostId} without permission.", userId, command.PostId);
                return Result.Forbidden(ErrorCodes.PostNoPermissionToDelete, "You do not have permission to delete this post.");
            }

            await repository.DeleteAsync(post);

            logger.LogInformation("Post {PostId} deleted by user {UserId}.", command.PostId, userId);

            return Result.Success();
        }
    }
}
