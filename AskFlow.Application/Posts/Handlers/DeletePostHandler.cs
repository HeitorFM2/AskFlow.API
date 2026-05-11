using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Posts.Command;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace AskFlow.Application.Posts.Handlers
{
    public class DeletePostHandler(
        IPostRepository repository,
        IHttpContextAccessor httpContextAccessor,
        ILogger<DeletePostHandler> logger) : IRequestHandler<DeletePostCommand, Result>
    {
        public async Task<Result> Handle(DeletePostCommand command, CancellationToken cancellationToken)
        {
            var userId = httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value
                ?? httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Result.Unauthorized(ErrorCodes.UserNotAuthenticated, "User not authenticated.");

            var post = await repository.FindByIdAsync(command.PostId);

            if (post is null)
                return Result.NotFound(ErrorCodes.PostNotFound, "Post not found.");

            if (post.UserId != userId)
            {
                logger.LogWarning("User {UserId} attempted to delete post {PostId} without permission.", userId, command.PostId);
                return Result.Unauthorized(ErrorCodes.PostNoPermissionToDelete, "You do not have permission to delete this post.");
            }

            await repository.DeleteAsync(post);

            logger.LogInformation("Post {PostId} deleted by user {UserId}.", command.PostId, userId);

            return Result.Success();
        }
    }
}
