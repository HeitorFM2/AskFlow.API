using System.Security.Cryptography;
using System.Text;

namespace AskFlow.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; private set; }
        public string TokenHash { get; private set; } = string.Empty;
        public DateTime ExpiresAt { get; private set; }
        public bool IsRevoked { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public string UserId { get; private set; } = string.Empty;
        public User User { get; private set; } = null!;

        protected RefreshToken() { }

        public RefreshToken(string token, User user, DateTime expiresAt)
        {
            TokenHash = HashToken(token);
            User = user;
            UserId = user.Id;
            ExpiresAt = expiresAt;
            CreatedAt = DateTime.UtcNow;
            IsRevoked = false;
        }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => !IsRevoked && !IsExpired;

        public void Revoke() => IsRevoked = true;

        public static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }
    }
}
