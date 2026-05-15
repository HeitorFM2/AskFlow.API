using AskFlow.Application.Interfaces;
using AskFlow.Domain.Entities;
using AskFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AskFlow.Infrastructure.Repositories
{
    public class PostRepository(AppDbContext context) : IPostRepository
    {
        private readonly AppDbContext _context = context;

        public Task<Post?> FindByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _context.Posts.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public void Add(Post post) => _context.Posts.Add(post);

        public void Delete(Post post) => post.MarkAsDeleted();
    }
}
