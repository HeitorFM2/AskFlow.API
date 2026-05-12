using Microsoft.AspNetCore.Identity;

namespace AskFlow.Domain.Entities
{
    public class User : IdentityUser
    {
        public required string Identification { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Post> Posts { get; set; } = [];
        public ICollection<Comment> Comments { get; set; } = [];
        public ICollection<Like> Likes { get; set; } = [];
    }
}
