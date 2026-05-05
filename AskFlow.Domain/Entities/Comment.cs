namespace AskFlow.Domain.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public int? PostId { get; private set; }
        public required Post Post { get; set; }

        public string UserId { get; set; } = string.Empty;
        public required User User { get; set; }
    }
}
