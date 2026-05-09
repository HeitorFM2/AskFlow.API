using AskFlow.Application.Users.ViewModels;

namespace AskFlow.Application.Comments.ViewModels
{
    public class CommentViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? ParentCommentId { get; set; }
        public int ReplyCount { get; set; }
        public required UserViewModel User { get; set; }
    }
}
