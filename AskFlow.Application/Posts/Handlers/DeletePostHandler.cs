using AskFlow.Application.Posts.Command;
using AskFlow.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AskFlow.Application.Posts.Handlers
{
    public class DeletePostHandler(
        IPostRepository repository,
        IHttpContextAccessor httpContextAccessor) : IRequestHandler<DeletePostCommand>
    {
        private readonly IPostRepository _repository = repository;

        public async Task Handle(DeletePostCommand command, CancellationToken cancellationToken)
        {
            using var transaction = await _repository.BeginTransactionAsync();

            try
            {
                var userId = httpContextAccessor.HttpContext!.User.FindFirst("sub")?.Value
                    ?? httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? throw new UnauthorizedAccessException("Usuário não autenticado.");

                var post = await _repository.GetByIdAsync(command.PostId)
                    ?? throw new Exception("Post não encontrado.");

                if (post.UserId != userId)
                    throw new UnauthorizedAccessException("Você não tem permissão para deletar este post.");

                await _repository.DeleteAsync(post);

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}