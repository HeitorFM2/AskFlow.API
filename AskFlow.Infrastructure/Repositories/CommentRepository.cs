using AskFlow.Application.Comments.ViewModels;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Users.ViewModels;
using AskFlow.Domain.Entities;
using AskFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AskFlow.Infrastructure.Repositories
{
    public class CommentRepository(AppDbContext context) : ICommentRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<IReadOnlyList<CommentViewModel>> GetByPostAsync(int postId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var data = await _context.Comments
                .AsNoTracking()
                .Where(c => c.PostId == postId && c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.CreatedAt,
                    c.ParentCommentId,
                    ReplyCount = c.Replies.Count(),
                    c.User.UserName,
                    c.User.Identification,
                    c.User.AvatarUrl
                })
                .ToListAsync(cancellationToken);

            return data.Select(c => new CommentViewModel
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                ParentCommentId = c.ParentCommentId,
                ReplyCount = c.ReplyCount,
                User = new UserDto
                {
                    UserName = c.UserName ?? "",
                    Identification = c.Identification,
                    AvatarUrl = c.AvatarUrl
                }
            }).ToList();
        }

        public async Task<IReadOnlyList<CommentViewModel>> GetRepliesAsync(int parentCommentId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var data = await _context.Comments
                .AsNoTracking()
                .Where(c => c.ParentCommentId == parentCommentId)
                .OrderBy(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.CreatedAt,
                    c.ParentCommentId,
                    ReplyCount = c.Replies.Count(),
                    c.User.UserName,
                    c.User.Identification,
                    c.User.AvatarUrl
                })
                .ToListAsync(cancellationToken);

            return data.Select(c => new CommentViewModel
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                ParentCommentId = c.ParentCommentId,
                ReplyCount = c.ReplyCount,
                User = new UserDto
                {
                    UserName = c.UserName ?? "",
                    Identification = c.Identification,
                    AvatarUrl = c.AvatarUrl
                }
            }).ToList();
        }

        public async Task<Comment?> GetByIdAsync(int commentId, CancellationToken cancellationToken = default)
        {
            return await _context.Comments
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
