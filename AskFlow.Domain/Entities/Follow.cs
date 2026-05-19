namespace AskFlow.Domain.Entities
{
    public class Follow
    {
        public string FollowerId { get; private set; } = string.Empty;
        public User Follower { get; private set; } = null!;

        public string FollowedId { get; private set; } = string.Empty;
        public User Followed { get; private set; } = null!;

        public DateTime CreatedAt { get; private set; }

        private Follow() { }

        public static Follow Create(string followerId, string followedId) => new()
        {
            FollowerId = followerId,
            FollowedId = followedId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
