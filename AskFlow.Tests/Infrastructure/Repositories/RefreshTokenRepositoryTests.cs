using AskFlow.Domain.Entities;
using AskFlow.Infrastructure.Repositories;
using AskFlow.Tests.Common.Builders;
using AskFlow.Tests.Common.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace AskFlow.Tests.Infrastructure.Repositories
{
    public class RefreshTokenRepositoryTests
    {
        [Fact]
        public async Task Add_AndGetByTokenAsync_ShouldRoundtrip()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var user = new UserBuilder().Build();
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            var repo = new RefreshTokenRepository(ctx);
            var rt = new RefreshToken("abc", user, DateTime.UtcNow.AddDays(1));
            repo.Add(rt);
            await ctx.SaveChangesAsync();

            var fetched = await repo.GetByTokenAsync("abc");
            fetched.Should().NotBeNull();
            fetched!.User.Id.Should().Be(user.Id);
        }

        [Fact]
        public async Task GetByTokenAsync_ShouldReturnNull_WhenMissing()
        {
            using var fx = new DatabaseFixture();
            var repo = new RefreshTokenRepository(fx.Context);

            var fetched = await repo.GetByTokenAsync("missing");
            fetched.Should().BeNull();
        }

        [Fact]
        public async Task RevokeAllByUserIdAsync_ShouldRevokeOnlyActive_ForGivenUser()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var u1 = new UserBuilder().Build();
            var u2 = new UserBuilder().Build();
            ctx.Users.AddRange(u1, u2);
            await ctx.SaveChangesAsync();

            var repo = new RefreshTokenRepository(ctx);
            repo.Add(new RefreshToken("a", u1, DateTime.UtcNow.AddDays(1)));
            repo.Add(new RefreshToken("b", u1, DateTime.UtcNow.AddDays(1)));
            repo.Add(new RefreshToken("c", u2, DateTime.UtcNow.AddDays(1)));
            await ctx.SaveChangesAsync();

            await repo.RevokeAllByUserIdAsync(u1.Id);
            await ctx.SaveChangesAsync();

            var hashA = RefreshToken.HashToken("a");
            var hashB = RefreshToken.HashToken("b");
            var hashC = RefreshToken.HashToken("c");
            (await ctx.RefreshTokens.SingleAsync(t => t.TokenHash == hashA)).IsRevoked.Should().BeTrue();
            (await ctx.RefreshTokens.SingleAsync(t => t.TokenHash == hashB)).IsRevoked.Should().BeTrue();
            (await ctx.RefreshTokens.SingleAsync(t => t.TokenHash == hashC)).IsRevoked.Should().BeFalse();
        }
    }
}
