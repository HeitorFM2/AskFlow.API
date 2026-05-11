namespace AskFlow.Domain.Entities
{
    public class Post
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

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
    }
}
