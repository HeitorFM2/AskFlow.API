using AskFlow.Application.Comments.ViewModels;

namespace AskFlow.Application.Interfaces
{
    public interface ICommentQueries
    {
        Task<IReadOnlyList<CommentViewModel>> GetByPostAsync(int postId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<CommentViewModel>> GetRepliesAsync(int parentCommentId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> CountByPostAsync(int postId, CancellationToken cancellationToken = default);
        Task<int> CountRepliesAsync(int parentCommentId, CancellationToken cancellationToken = default);
    }
}
