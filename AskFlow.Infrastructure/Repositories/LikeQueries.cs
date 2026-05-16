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
            return await _context.Posts
                .AsNoTracking()
                .Where(p => p.Likes.Any(l => l.UserId == userId))
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new LikedPostDto
                {
                    PostId = p.Id,
                    Content = p.Content,
                    CreatedAt = p.CreatedAt,
                    CommentsCount = p.CommentCount,
                    LikesCount = p.LikeCount,
                    AuthorUserName = p.User.UserName ?? string.Empty,
                    AuthorIdentification = p.User.Identification,
                    AuthorAvatarUrl = p.User.AvatarUrl
                })
                .ToListAsync(cancellationToken);
        }

        public Task<int> CountLikedPostAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            return _context.Posts.CountAsync(p => p.Likes.Any(l => l.UserId == userId), cancellationToken);
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
