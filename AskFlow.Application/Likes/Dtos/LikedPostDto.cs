namespace AskFlow.Application.Likes.Dtos
{
    public class LikedPostDto
    {
        public int PostId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int CommentsCount { get; set; }
        public int LikesCount { get; set; }
        public string AuthorUserName { get; set; } = string.Empty;
        public string AuthorIdentification { get; set; } = string.Empty;
        public string? AuthorAvatarUrl { get; set; }
    }
}
