using AskFlow.Application.Interfaces;
using AskFlow.Domain.Entities;
using AskFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AskFlow.Infrastructure.Repositories
{
    public class FollowRepository(AppDbContext context) : IFollowRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<bool> ToggleAsync(string followerId, string followedId, CancellationToken cancellationToken = default)
        {
            var existing = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowedId == followedId, cancellationToken);

            if (existing is not null)
            {
                _context.Follows.Remove(existing);
                return false;
            }

            _context.Follows.Add(Follow.Create(followerId, followedId));

            return true;
        }
    }
}
