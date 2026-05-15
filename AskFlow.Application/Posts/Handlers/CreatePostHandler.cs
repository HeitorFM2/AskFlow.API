using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Posts.Command;
using AskFlow.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AskFlow.Application.Posts.Handlers
{
    public class CreatePostHandler(
        IPostRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<CreatePostHandler> logger) : IRequestHandler<CreatePostCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(CreatePostCommand command, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Result<int>.Unauthorized(ErrorCodes.UserNotAuthenticated, "User not authenticated.");

            var post = new Post(command.Content, userId);
            repository.Add(post);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Post {PostId} created by user {UserId}.", post.Id, userId);

            return Result<int>.Success(post.Id);
        }
    }
}
