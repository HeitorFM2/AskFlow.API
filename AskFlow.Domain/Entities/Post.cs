namespace AskFlow.Domain.Entities
{
    public class Post
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int CommentCount { get; private set; }
        public int LikeCount { get; private set; }

        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }

        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;

        public ICollection<Comment> Comments { get; set; } = [];
        public ICollection<Like> Likes { get; set; } = [];

        protected Post() { }

        public Post(string content, string userId)
        {
            Content = content;
            UserId = userId;
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkAsDeleted()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }

        public void IncrementCommentCount() => CommentCount++;

        public void DecrementCommentCount()
        {
            if (CommentCount > 0) CommentCount--;
        }

        public void IncrementLikeCount() => LikeCount++;

        public void DecrementLikeCount()
        {
            if (LikeCount > 0) LikeCount--;
        }
    }
}
