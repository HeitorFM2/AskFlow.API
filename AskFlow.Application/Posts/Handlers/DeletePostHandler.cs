using AskFlow.Application.Common;
using AskFlow.Application.Posts.Command;
using AskFlow.Domain.Interfaces;
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
                return Result.Unauthorized("Usuário não autenticado.");

            var post = await repository.GetByIdAsync(command.PostId);

            if (post is null)
                return Result.NotFound("Post não encontrado.");

            if (post.UserId != userId)
            {
                logger.LogWarning("Usuário {UserId} tentou deletar post {PostId} sem permissão.", userId, command.PostId);
                return Result.Unauthorized("Você não tem permissão para deletar este post.");
            }

            await repository.DeleteAsync(post);

            logger.LogInformation("Post {PostId} deletado pelo usuário {UserId}.", command.PostId, userId);

            return Result.Success();
        }
    }
}
