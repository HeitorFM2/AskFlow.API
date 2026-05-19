using AskFlow.Application.Users.ViewModels;

namespace AskFlow.Application.Interfaces
{
    public interface IUserQueries
    {
        Task<IReadOnlyList<UserDto>> GetAllAsync(string excludeUserId, string? search = null, CancellationToken cancellationToken = default);
    }
}
