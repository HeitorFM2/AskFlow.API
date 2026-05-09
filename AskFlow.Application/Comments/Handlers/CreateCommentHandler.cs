using AskFlow.Application.Comments.Commands;
using AskFlow.Application.Common;
using AskFlow.Domain.Entities;
using AskFlow.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace AskFlow.Application.Comments.Handlers
{
    public class CreateCommentHandler(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IHttpContextAccessor httpContextAccessor,
        ILogger<CreateCommentHandler> logger) : IRequestHandler<CreateCommentCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(CreateCommentCommand command, CancellationToken cancellationToken)
        {
            var userId = httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value
                ?? httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Result<int>.Unauthorized("Usuário não autenticado.");

            var post = await postRepository.GetByIdAsync(command.PostId);
            if (post is null)
                return Result<int>.NotFound("Post não encontrado.");

            if (command.ParentCommentId.HasValue)
            {
                var parent = await commentRepository.GetByIdAsync(command.ParentCommentId.Value, cancellationToken);
                if (parent is null)
                    return Result<int>.NotFound("Comentário pai não encontrado.");
            }

            var comment = new Comment(command.Content, userId, command.PostId, command.ParentCommentId);
            await commentRepository.AddAsync(comment, cancellationToken);

            logger.LogInformation("Comentário {CommentId} criado pelo usuário {UserId} no post {PostId}.", comment.Id, userId, command.PostId);

            return Result<int>.Success(comment.Id);
        }
    }
}
