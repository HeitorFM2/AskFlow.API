namespace AskFlow.Domain.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public int? PostId { get; set; }
        public Post Post { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;

        public int? ParentCommentId { get; set; }
        public Comment? ParentComment { get; set; }
        public ICollection<Comment> Replies { get; set; } = [];

        protected Comment() { }

        public Comment(string content, string userId, int? postId, int? parentCommentId = null)
        {
            Content = content;
            UserId = userId;
            PostId = postId;
            ParentCommentId = parentCommentId;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
