using AskFlow.Application.Follows.ViewModels;

namespace AskFlow.Application.Interfaces
{
    public interface IFollowQueries
    {
        Task<IReadOnlyList<FollowViewModel>> GetFollowersAsync(string userId, int page, int pageSize, string? search = null, CancellationToken cancellationToken = default);
        Task<int> CountFollowersAsync(string userId, string? search = null, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<FollowViewModel>> GetFollowingAsync(string userId, int page, int pageSize, string? search = null, CancellationToken cancellationToken = default);
        Task<int> CountFollowingAsync(string userId, string? search = null, CancellationToken cancellationToken = default);
        Task<HashSet<string>> GetFollowedUserNamesAsync(string followerId, IEnumerable<string> userNames, CancellationToken cancellationToken = default);
        Task<bool> IsFollowingAsync(string followerId, string targetUserName, CancellationToken cancellationToken = default);
    }
}
