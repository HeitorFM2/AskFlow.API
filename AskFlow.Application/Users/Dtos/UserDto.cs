namespace AskFlow.Application.Users.ViewModels
{
    public class UserDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Identification { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public bool IsFollowing { get; set; }
    }
}
