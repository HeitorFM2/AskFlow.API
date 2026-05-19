namespace AskFlow.Application.Follows.ViewModels
{
    public class FollowViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string Identification { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public bool IsFollowing { get; set; }
    }
}
