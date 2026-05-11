using AskFlow.Application.Interfaces;
using AskFlow.Application.Likes.Dtos;
using AskFlow.Domain.Entities;
using AskFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AskFlow.Infrastructure.Repositories
{
    public class LikeRepository(AppDbContext context) : ILikeRepository
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
                    CommentsCount = l.Post.Comments.Count(),
                    LikesCount = l.Post.Likes.Count(),
                    AuthorUserName = l.Post.User.UserName ?? string.Empty,
                    AuthorIdentification = l.Post.User.Identification
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountLikedPostAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Likes
                .CountAsync(c => c.UserId == userId, cancellationToken);
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

        public async Task<bool> ToggleLikeAsync(
            string userId,
            int postId,
            CancellationToken cancellationToken = default)
        {
            var deleted = await _context.Likes
                .Where(l => l.UserId == userId && l.PostId == postId)
                .ExecuteDeleteAsync(cancellationToken);

            if (deleted > 0) return false;

            var post = await _context.Posts.FindAsync([postId], cancellationToken)
                ?? throw new InvalidOperationException($"Post {postId} not found.");

            var userRef = _context.Users.Local.FirstOrDefault(u => u.Id == userId)
                ?? _context.Attach(new User { Id = userId, Identification = string.Empty }).Entity;

            _context.Likes.Add(new Like(postId, userId)
            {
                Post = post,
                User = userRef
            });

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
