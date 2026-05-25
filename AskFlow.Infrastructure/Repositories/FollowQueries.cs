using AskFlow.Application.Follows.ViewModels;
using AskFlow.Application.Interfaces;
using AskFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AskFlow.Infrastructure.Repositories
{
    public class FollowQueries(AppDbContext context) : IFollowQueries
    {
        private readonly AppDbContext _context = context;

        public async Task<IReadOnlyList<FollowViewModel>> GetFollowersAsync(
            string userId, int page, int pageSize, string? search = null, CancellationToken cancellationToken = default)
        {
            return await _context.Follows
                .AsNoTracking()
                .Where(f => f.FollowedId == userId
                    && (search == null || f.Follower.UserName!.Contains(search) || f.Follower.Identification.Contains(search)))
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FollowViewModel
                {
                    UserName = f.Follower.UserName ?? string.Empty,
                    Identification = f.Follower.Identification,
                    AvatarUrl = f.Follower.AvatarUrl
                })
                .ToListAsync(cancellationToken);
        }

        public Task<int> CountFollowersAsync(string userId, string? search = null, CancellationToken cancellationToken = default)
        {
            return _context.Follows.CountAsync(f => f.FollowedId == userId
                && (search == null || f.Follower.UserName!.Contains(search) || f.Follower.Identification.Contains(search)), cancellationToken);
        }

        public async Task<IReadOnlyList<FollowViewModel>> GetFollowingAsync(
            string userId, int page, int pageSize, string? search = null, CancellationToken cancellationToken = default)
        {
            return await _context.Follows
                .AsNoTracking()
                .Where(f => f.FollowerId == userId
                    && (search == null || f.Followed.UserName!.Contains(search) || f.Followed.Identification.Contains(search)))
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FollowViewModel
                {
                    UserName = f.Followed.UserName ?? string.Empty,
                    Identification = f.Followed.Identification,
                    AvatarUrl = f.Followed.AvatarUrl,
                    IsFollowing = true
                })
                .ToListAsync(cancellationToken);
        }

        public Task<int> CountFollowingAsync(string userId, string? search = null, CancellationToken cancellationToken = default)
        {
            return _context.Follows.CountAsync(f => f.FollowerId == userId
                && (search == null || f.Followed.UserName!.Contains(search) || f.Followed.Identification.Contains(search)), cancellationToken);
        }

        public Task<bool> IsFollowingAsync(string followerId, string targetUserName, CancellationToken cancellationToken = default)
        {
            return _context.Follows
                .AsNoTracking()
                .AnyAsync(f => f.FollowerId == followerId && f.Followed.UserName == targetUserName, cancellationToken);
        }

        public async Task<HashSet<string>> GetFollowedUserNamesAsync(
            string followerId,
            IEnumerable<string> userNames,
            CancellationToken cancellationToken = default)
        {
            var names = userNames as List<string> ?? [.. userNames];

            if (names.Count == 0)
                return [];

            return [.. (await _context.Follows
                .AsNoTracking()
                .Where(f => f.FollowerId == followerId && names.Contains(f.Followed.UserName!))
                .Select(f => f.Followed.UserName!)
                .ToListAsync(cancellationToken))];
        }
    }
}
