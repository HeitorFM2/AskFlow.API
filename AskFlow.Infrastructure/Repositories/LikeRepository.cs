using AskFlow.Application.Interfaces;
using AskFlow.Domain.Entities;
using AskFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AskFlow.Infrastructure.Repositories
{
    public class LikeRepository(AppDbContext context) : ILikeRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<bool> ToggleAsync(Post post, string userId, CancellationToken cancellationToken = default)
        {
            var existing = await _context.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == post.Id, cancellationToken);

            if (existing is not null)
            {
                _context.Likes.Remove(existing);
                post.DecrementLikeCount();
                return false;
            }

            var userRef = _context.Users.Local.FirstOrDefault(u => u.Id == userId)
                ?? _context.Attach(new User { Id = userId, Identification = string.Empty }).Entity;

            _context.Likes.Add(new Like(post.Id, userId)
            {
                Post = post,
                User = userRef
            });

            post.IncrementLikeCount();
            return true;
        }
    }
}
