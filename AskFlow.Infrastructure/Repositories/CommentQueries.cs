using AskFlow.Application.Comments.ViewModels;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Users.ViewModels;
using AskFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AskFlow.Infrastructure.Repositories
{
    public class CommentQueries(AppDbContext context) : ICommentQueries
    {
        private readonly AppDbContext _context = context;

        public async Task<IReadOnlyList<CommentViewModel>> GetByPostAsync(int postId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _context.Comments
                .AsNoTracking()
                .Where(c => c.PostId == postId && c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CommentViewModel
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    ParentCommentId = c.ParentCommentId,
                    ReplyCount = c.Replies.Count,
                    User = new UserDto
                    {
                        UserName = c.User.UserName ?? "",
                        Identification = c.User.Identification,
                        AvatarUrl = c.User.AvatarUrl
                    }
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<CommentViewModel>> GetRepliesAsync(int parentCommentId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _context.Comments
                .AsNoTracking()
                .Where(c => c.ParentCommentId == parentCommentId)
                .OrderBy(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CommentViewModel
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    ParentCommentId = c.ParentCommentId,
                    ReplyCount = c.Replies.Count,
                    User = new UserDto
                    {
                        UserName = c.User.UserName ?? "",
                        Identification = c.User.Identification,
                        AvatarUrl = c.User.AvatarUrl
                    }
                })
                .ToListAsync(cancellationToken);
        }

        public Task<int> CountByPostAsync(int postId, CancellationToken cancellationToken = default)
        {
            return _context.Comments
                .CountAsync(c => c.PostId == postId && c.ParentCommentId == null, cancellationToken);
        }

        public Task<int> CountRepliesAsync(int parentCommentId, CancellationToken cancellationToken = default)
        {
            return _context.Comments
                .CountAsync(c => c.ParentCommentId == parentCommentId, cancellationToken);
        }
    }
}
