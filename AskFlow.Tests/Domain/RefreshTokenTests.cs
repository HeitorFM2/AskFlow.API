using AskFlow.Domain.Entities;
using AskFlow.Tests.Common.Builders;

namespace AskFlow.Tests.Domain
{
    public class RefreshTokenTests
    {
        [Fact]
        public void Constructor_ShouldSet_AllProperties()
        {
            var user = new UserBuilder().Build();
            var expiresAt = DateTime.UtcNow.AddDays(7);

            var token = new RefreshToken("xyz", user, expiresAt);

            token.TokenHash.Should().Be(RefreshToken.HashToken("xyz"));
            token.User.Should().Be(user);
            token.UserId.Should().Be(user.Id);
            token.ExpiresAt.Should().Be(expiresAt);
            token.IsRevoked.Should().BeFalse();
            token.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void ParameterlessConstructor_ShouldCreateInstance_WithDefaults()
        {
            var token = (RefreshToken)Activator.CreateInstance(typeof(RefreshToken), nonPublic: true)!;

            token.Should().NotBeNull();
            token.Id.Should().Be(0);
            token.TokenHash.Should().BeEmpty();
            token.UserId.Should().BeEmpty();
            token.IsRevoked.Should().BeFalse();
            token.ExpiresAt.Should().Be(default);
            token.CreatedAt.Should().Be(default);
        }

        [Fact]
        public void IsActive_ShouldBeTrue_WhenNotRevoked_AndNotExpired()
        {
            var token = new RefreshTokenBuilder()
                .WithExpiresAt(DateTime.UtcNow.AddDays(1))
                .Build();

            token.IsActive.Should().BeTrue();
            token.IsExpired.Should().BeFalse();
        }

        [Fact]
        public void IsExpired_ShouldBeTrue_WhenExpiresAtPassed()
        {
            var token = new RefreshTokenBuilder()
                .WithExpiresAt(DateTime.UtcNow.AddSeconds(-1))
                .Build();

            token.IsExpired.Should().BeTrue();
            token.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Revoke_ShouldFlag_RevokedAndDeactivate()
        {
            var token = new RefreshTokenBuilder()
                .WithExpiresAt(DateTime.UtcNow.AddDays(1))
                .Build();

            token.Revoke();

            token.IsRevoked.Should().BeTrue();
            token.IsActive.Should().BeFalse();
        }
    }
}
