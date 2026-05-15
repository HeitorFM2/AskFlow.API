using AskFlow.Domain.Entities;

namespace AskFlow.Application.Interfaces
{
    public interface ICommentRepository
    {
        Task<Comment?> FindByIdAsync(int commentId, CancellationToken cancellationToken = default);
        void Add(Comment comment);
        void Delete(Comment comment);
    }
}
