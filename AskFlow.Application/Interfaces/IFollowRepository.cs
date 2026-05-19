namespace AskFlow.Application.Interfaces
{
    public interface IFollowRepository
    {
        Task<bool> ToggleAsync(string followerId, string followedId, CancellationToken cancellationToken = default);
    }
}
