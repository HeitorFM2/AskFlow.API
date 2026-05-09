using AskFlow.Domain.Entities;

namespace AskFlow.Domain.Interfaces
{
    public interface ICommentRepository
    {
        Task<IReadOnlyList<Comment>> GetByPostAsync(int postId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Comment>> GetRepliesAsync(int parentCommentId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<Comment?> GetByIdAsync(int commentId, CancellationToken cancellationToken = default);
        Task<int> CountByPostAsync(int postId, CancellationToken cancellationToken = default);
        Task<int> CountRepliesAsync(int parentCommentId, CancellationToken cancellationToken = default);
        Task AddAsync(Comment comment, CancellationToken cancellationToken = default);
        Task DeleteAsync(Comment comment, CancellationToken cancellationToken = default);
    }
}