namespace AskFlow.Application.Auth.ViewModels
{
    public class AuthViewModel
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserAuthViewModel User { get; set; } = null!;
    }

    public class UserAuthViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Identification { get; set; } = string.Empty;
    }
}
