using AskFlow.Domain.Entities;

namespace AskFlow.Application.Interfaces
{
    public interface IPasswordSignInService
    {
        Task<PasswordSignInResult> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken);
    }

    public enum PasswordSignInResult
    {
        Success,
        Failed,
        LockedOut
    }
}
