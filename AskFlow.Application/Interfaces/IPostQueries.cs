using AskFlow.Application.Posts.ViewModels;

namespace AskFlow.Application.Interfaces
{
    public interface IPostQueries
    {
        Task<IReadOnlyList<PostsViewModel>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
        Task<PostViewModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PostsViewModel>> GetByUserAsync(string userId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> CountByUserAsync(string userId, CancellationToken cancellationToken = default);
    }
}
