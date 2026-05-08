using AskFlow.Application.Posts.Command;
using AskFlow.Domain.Entities;
using AskFlow.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AskFlow.Application.Posts.Handlers
{
    public class CreatePostHandler(
        IPostRepository repository,
        IHttpContextAccessor httpContextAccessor) : IRequestHandler<CreatePostCommand, int>
    {
        private readonly IPostRepository _repository = repository;

        public async Task<int> Handle(
            CreatePostCommand command,
            CancellationToken cancellationToken)
        {
            using var transaction = await _repository.BeginTransactionAsync();

            try
            {
                var userId = httpContextAccessor.HttpContext!.User.FindFirst("sub")?.Value
                    ?? httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? throw new UnauthorizedAccessException("Usuário não autenticado.");

                var post = new Post(command.Content, userId);
                await _repository.AddAsync(post);

                await transaction.CommitAsync(cancellationToken);

                return post.Id;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}