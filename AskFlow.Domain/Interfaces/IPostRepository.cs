using AskFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace AskFlow.Domain.Interfaces
{
    public interface IPostRepository
    {
        Task<IDbContextTransaction> BeginTransactionAsync();
        IQueryable<Post> GetAll();
        Task<Post?> GetByIdAsync(int id);
        Task AddAsync(Post post);
        Task DeleteAsync(Post post);
    }
}
