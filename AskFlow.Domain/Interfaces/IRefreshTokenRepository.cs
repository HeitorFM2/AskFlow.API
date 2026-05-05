using AskFlow.Domain.Entities;

namespace AskFlow.Domain.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task AddAsync(RefreshToken refreshToken);
        Task RevokeAllByUserIdAsync(string userId);
    }
}
