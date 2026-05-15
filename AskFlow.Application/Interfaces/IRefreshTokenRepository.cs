using AskFlow.Domain.Entities;

namespace AskFlow.Application.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
        void Add(RefreshToken refreshToken);
        Task RevokeAllByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    }
}
