using AskFlow.Domain.Entities;

namespace AskFlow.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        int RefreshTokenExpiresInDays { get; }
        DateTime GetAccessTokenExpiry();
    }
}
