using AskFlow.Application.Interfaces;
using AskFlow.Domain.Entities;
using AskFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AskFlow.Infrastructure.Repositories
{
    public class CommentRepository(AppDbContext context) : ICommentRepository
    {
        private readonly AppDbContext _context = context;

        public Task<Comment?> FindByIdAsync(int commentId, CancellationToken cancellationToken = default)
        {
            return _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);
        }

        public void Add(Comment comment) => _context.Comments.Add(comment);

        public void Delete(Comment comment) => _context.Comments.Remove(comment);
    }
}
