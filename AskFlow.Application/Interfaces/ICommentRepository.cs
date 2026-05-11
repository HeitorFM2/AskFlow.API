using AskFlow.Application.Comments.ViewModels;
using AskFlow.Domain.Entities;

namespace AskFlow.Application.Interfaces
{
    public interface ICommentRepository
    {
        Task<IReadOnlyList<CommentViewModel>> GetByPostAsync(int postId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<CommentViewModel>> GetRepliesAsync(int parentCommentId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<Comment?> GetByIdAsync(int commentId, CancellationToken cancellationToken = default);
        Task<int> CountByPostAsync(int postId, CancellationToken cancellationToken = default);
        Task<int> CountRepliesAsync(int parentCommentId, CancellationToken cancellationToken = default);
        Task AddAsync(Comment comment, CancellationToken cancellationToken = default);
        Task DeleteAsync(Comment comment, CancellationToken cancellationToken = default);
    }
}
