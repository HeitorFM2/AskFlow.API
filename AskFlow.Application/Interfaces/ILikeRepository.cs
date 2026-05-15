using AskFlow.Domain.Entities;

namespace AskFlow.Application.Interfaces
{
    public interface ILikeRepository
    {
        Task<bool> ToggleAsync(Post post, string userId, CancellationToken cancellationToken = default);
    }
}
