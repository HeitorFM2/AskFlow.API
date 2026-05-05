namespace AskFlow.Domain.Entities
{
    public class Post
    {
        public int Id { get; set; }
        public required string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; private set; } = false;
        public DateTime? DeletedAt { get; set; }

        public string UserId { get; set; } = string.Empty;
        public required User User { get; set; }

        public ICollection<Comment> Comments { get; set; } = [];
        public ICollection<Like> Likes { get; set; } = [];
    }
}
