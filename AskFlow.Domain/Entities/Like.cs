namespace AskFlow.Domain.Entities
{
    public class Like
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }

        public int? PostId { get; private set; }
        public required Post Post { get; set; }

        public string UserId { get; set; } = string.Empty;
        public required User User { get; set; }
    }
}
