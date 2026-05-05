using AskFlow.Application.User.ViewModels;

namespace AskFlow.Application.Posts.ViewModels
{
    public class PostsViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int Comments { get; set; }
        public int Likes { get; set; }
        public required UserViewModel User { get; set; }
    }
}
