using AskFlow.Domain.Entities;

namespace AskFlow.Domain.Interfaces
{
    public interface IPostRepository
    {
        Task<IReadOnlyList<Post>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
        Task<Post?> GetByIdAsync(int id);
        Task AddAsync(Post post);
        Task DeleteAsync(Post post);
    }
}
