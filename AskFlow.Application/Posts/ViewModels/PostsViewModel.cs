using AskFlow.Application.Comments.ViewModels;
using AskFlow.Application.Users.ViewModels;

namespace AskFlow.Application.Posts.ViewModels
{
    public class PostsViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int Comments { get; set; }
        public int Likes { get; set; }
        public bool IsLiked { get; set; }
        public required UserDto User { get; set; }
    }

    public class PostViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int Likes { get; set; }
        public required IEnumerable<CommentViewModel> Comments { get; set; }
        public required UserDto User { get; set; }
    }
}
