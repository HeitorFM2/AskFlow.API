using AskFlow.Application.Comments.Commands;
using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AskFlow.Application.Comments.Handlers
{
    public class CreateCommentHandler(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<CreateCommentHandler> logger) : IRequestHandler<CreateCommentCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(CreateCommentCommand command, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Result<int>.Unauthorized(ErrorCodes.UserNotAuthenticated, "User not authenticated.");

            var post = await postRepository.FindByIdAsync(command.PostId, cancellationToken);
            if (post is null)
                return Result<int>.NotFound(ErrorCodes.PostNotFound, "Post not found.");

            if (command.ParentCommentId.HasValue)
            {
                var parent = await commentRepository.FindByIdAsync(command.ParentCommentId.Value, cancellationToken);
                if (parent is null)
                    return Result<int>.NotFound(ErrorCodes.CommentParentNotFound, "Parent comment not found.");
            }

            var comment = new Comment(command.Content, userId, command.PostId, command.ParentCommentId);
            commentRepository.Add(comment);
            post.IncrementCommentCount();

            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Comment {CommentId} created by user {UserId} on post {PostId}.", comment.Id, userId, command.PostId);

            return Result<int>.Success(comment.Id);
        }
    }
}
