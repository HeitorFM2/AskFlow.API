using AskFlow.Domain.Entities;

namespace AskFlow.Domain.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}
