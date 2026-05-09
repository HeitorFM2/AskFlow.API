using AskFlow.Application.Comments.Commands;
using AskFlow.Application.Common;
using AskFlow.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AskFlow.Application.Comments.Handlers
{
    public class DeleteCommentHandler(
        ICommentRepository repository,
        IHttpContextAccessor httpContextAccessor) : IRequestHandler<DeleteCommentCommand, Result>
    {
        public async Task<Result> Handle(DeleteCommentCommand command, CancellationToken cancellationToken)
        {
            var userId = httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value
                ?? httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Result.Unauthorized("Usuário não autenticado.");

            var comment = await repository.GetByIdAsync(command.CommentId, cancellationToken);
            if (comment is null)
                return Result.NotFound("Comentário não encontrado.");

            if (comment.UserId != userId)
                return Result.Unauthorized("Sem permissão para deletar este comentário.");

            await repository.DeleteAsync(comment, cancellationToken);
            return Result.Success();
        }
    }
}
