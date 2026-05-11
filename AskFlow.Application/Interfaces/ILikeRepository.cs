using AskFlow.Application.Likes.Dtos;

namespace AskFlow.Application.Interfaces
{
    public interface ILikeRepository
    {
        Task<IReadOnlyList<LikedPostDto>> GetLikePostsAsync(
            string userId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<int> CountLikedPostAsync(
            string userId, 
            CancellationToken cancellationToken = default);

        Task<HashSet<int>> GetLikedPostIdsAsync(
            string userId, 
            IEnumerable<int> postIds, 
            CancellationToken cancellationToken = default);

        Task<bool> ToggleLikeAsync(
            string userId,
            int postId,
            CancellationToken cancellationToken = default);
    }
}