using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Posts.Command;
using AskFlow.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace AskFlow.Application.Posts.Handlers
{
    public class CreatePostHandler(
        IPostRepository repository,
        IHttpContextAccessor httpContextAccessor,
        ILogger<CreatePostHandler> logger) : IRequestHandler<CreatePostCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(CreatePostCommand command, CancellationToken cancellationToken)
        {
            var userId = httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value
                ?? httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Result<int>.Unauthorized(ErrorCodes.UserNotAuthenticated, "User not authenticated.");

            var post = new Post(command.Content, userId);
            await repository.AddAsync(post);

            logger.LogInformation("Post {PostId} created by user {UserId}.", post.Id, userId);

            return Result<int>.Success(post.Id);
        }
    }
}
