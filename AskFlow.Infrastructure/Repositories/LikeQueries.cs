using AskFlow.Application.Interfaces;
using AskFlow.Application.Likes.Dtos;
using AskFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AskFlow.Infrastructure.Repositories
{
    public class LikeQueries(AppDbContext context) : ILikeQueries
    {
        private readonly AppDbContext _context = context;

        public async Task<IReadOnlyList<LikedPostDto>> GetLikePostsAsync(
            string userId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            return await _context.Likes
                .AsNoTracking()
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Post.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new LikedPostDto
                {
                    PostId = l.Post.Id,
                    Content = l.Post.Content,
                    CreatedAt = l.Post.CreatedAt,
                    CommentsCount = l.Post.CommentCount,
                    LikesCount = l.Post.LikeCount,
                    AuthorUserName = l.Post.User.UserName ?? string.Empty,
                    AuthorIdentification = l.Post.User.Identification
                })
                .ToListAsync(cancellationToken);
        }

        public Task<int> CountLikedPostAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            return _context.Likes.CountAsync(c => c.UserId == userId, cancellationToken);
        }

        public async Task<HashSet<int>> GetLikedPostIdsAsync(
            string userId,
            IEnumerable<int> postIds,
            CancellationToken cancellationToken = default)
        {
            var ids = postIds as List<int> ?? [.. postIds];

            if (ids.Count == 0)
                return [];

            return [.. (await _context.Likes
                .AsNoTracking()
                .Where(l => l.UserId == userId && l.PostId.HasValue && ids.Contains(l.PostId.Value))
                .Select(l => l.PostId!.Value)
                .ToListAsync(cancellationToken))];
        }
    }
}
