using AskFlow.Domain.Entities;
using AskFlow.Domain.Interfaces;
using AskFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AskFlow.Infrastructure.Repositories
{
    public class CommentRepository(AppDbContext context) : ICommentRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<IReadOnlyList<Comment>> GetByPostAsync(int postId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _context.Comments
                .Where(c => c.PostId == postId && c.ParentCommentId == null)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Comment>> GetRepliesAsync(int parentCommentId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _context.Comments
                .Where(c => c.ParentCommentId == parentCommentId)
                .Include(c => c.User)
                .OrderBy(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<Comment?> GetByIdAsync(int commentId, CancellationToken cancellationToken = default)
        {
            return await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);
        }

        public async Task<int> CountByPostAsync(int postId, CancellationToken cancellationToken = default)
        {
            return await _context.Comments
                .CountAsync(c => c.PostId == postId && c.ParentCommentId == null, cancellationToken);
        }

        public async Task<int> CountRepliesAsync(int parentCommentId, CancellationToken cancellationToken = default)
        {
            return await _context.Comments
                .CountAsync(c => c.ParentCommentId == parentCommentId, cancellationToken);
        }

        public async Task<IReadOnlyDictionary<int, int>> GetReplyCountsAsync(IReadOnlyCollection<int> commentIds, CancellationToken cancellationToken = default)
        {
            if (commentIds.Count == 0)
                return new Dictionary<int, int>();

            return await _context.Comments
                .Where(c => c.ParentCommentId.HasValue && commentIds.Contains(c.ParentCommentId.Value))
                .GroupBy(c => c.ParentCommentId!.Value)
                .Select(g => new { ParentId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ParentId, x => x.Count, cancellationToken);
        }

        public async Task AddAsync(Comment comment, CancellationToken cancellationToken = default)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Comment comment, CancellationToken cancellationToken = default)
        {
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
