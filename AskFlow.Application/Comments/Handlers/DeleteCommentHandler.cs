using AskFlow.Application.Comments.Commands;
using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using MediatR;

namespace AskFlow.Application.Comments.Handlers
{
    public class DeleteCommentHandler(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService) : IRequestHandler<DeleteCommentCommand, Result>
    {
        public async Task<Result> Handle(DeleteCommentCommand command, CancellationToken cancellationToken)
        {
            var userId = currentUserService.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Result.Unauthorized(ErrorCodes.UserNotAuthenticated, "User not authenticated.");

            var comment = await commentRepository.FindByIdAsync(command.CommentId, cancellationToken);
            if (comment is null)
                return Result.NotFound(ErrorCodes.CommentNotFound, "Comment not found.");

            if (comment.UserId != userId)
                return Result.Forbidden(ErrorCodes.CommentNoPermissionToDelete, "No permission to delete this comment.");

            commentRepository.Delete(comment);

            if (comment.PostId.HasValue)
            {
                var post = await postRepository.FindByIdAsync(comment.PostId.Value, cancellationToken);
                post?.DecrementCommentCount();
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
