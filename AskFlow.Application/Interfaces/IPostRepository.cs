using AskFlow.Domain.Entities;

namespace AskFlow.Application.Interfaces
{
    public interface IPostRepository
    {
        Task<Post?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
        void Add(Post post);
        void Delete(Post post);
    }
}
