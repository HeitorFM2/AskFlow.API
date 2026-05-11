using AskFlow.Application.Interfaces;
using AskFlow.Domain.Entities;
using AskFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AskFlow.Infrastructure.Repositories
{
    public class RefreshTokenRepository(AppDbContext context) : IRefreshTokenRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            var hash = RefreshToken.HashToken(token);

            return await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.TokenHash == hash);
        }

        public async Task AddAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task RevokeAllByUserIdAsync(string userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(r => r.UserId == userId && !r.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
                token.Revoke();

            await _context.SaveChangesAsync();
        }
    }
}
