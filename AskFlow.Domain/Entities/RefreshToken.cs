namespace AskFlow.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; private set; }
        public string Token { get; private set; } = string.Empty;
        public DateTime ExpiresAt { get; private set; }
        public bool IsRevoked { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public string UserId { get; private set; } = string.Empty;
        public User User { get; set; } = null!;

        protected RefreshToken() { }

        public RefreshToken(string token, User user, DateTime expiresAt)
        {
            Token = token;
            User = user;
            UserId = user.Id;
            ExpiresAt = expiresAt;
            CreatedAt = DateTime.UtcNow;
            IsRevoked = false;
        }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => !IsRevoked && !IsExpired;

        public void Revoke()
        {
            IsRevoked = true;
        }
    }
}
