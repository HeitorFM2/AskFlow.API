using AskFlow.Application.Posts.ViewModels;
using AskFlow.Domain.Entities;

namespace AskFlow.Application.Interfaces
{
    public interface IPostRepository
    {
        Task<IReadOnlyList<PostsViewModel>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
        Task<PostViewModel?> GetByIdAsync(int id);
        Task<Post?> FindByIdAsync(int id);
        Task AddAsync(Post post);
        Task DeleteAsync(Post post);
    }
}
