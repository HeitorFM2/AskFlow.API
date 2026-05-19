using AskFlow.Domain.Entities;
using AskFlow.Infrastructure.Repositories;
using AskFlow.Tests.Common.Builders;
using AskFlow.Tests.Common.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace AskFlow.Tests.Infrastructure.Repositories
{
    public class FollowRepositoryTests
    {
        private static async Task<(AskFlow.Infrastructure.Data.AppDbContext ctx, User follower, User followed)> SeedAsync(DatabaseFixture fx)
        {
            var ctx = fx.Context;
            var follower = new UserBuilder().Build();
            var followed = new UserBuilder().Build();
            ctx.Users.AddRange(follower, followed);
            await ctx.SaveChangesAsync();
            return (ctx, follower, followed);
        }

        [Fact]
        public async Task ToggleAsync_ShouldAddFollow_AndReturnTrue_WhenNotFollowing()
        {
            using var fx = new DatabaseFixture();
            var (ctx, follower, followed) = await SeedAsync(fx);
            var repo = new FollowRepository(ctx);

            var result = await repo.ToggleAsync(follower.Id, followed.Id);
            await ctx.SaveChangesAsync();

            result.Should().BeTrue();
            (await ctx.Follows.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task ToggleAsync_ShouldRemoveFollow_AndReturnFalse_WhenAlreadyFollowing()
        {
            using var fx = new DatabaseFixture();
            var (ctx, follower, followed) = await SeedAsync(fx);
            ctx.Follows.Add(Follow.Create(follower.Id, followed.Id));
            await ctx.SaveChangesAsync();

            var repo = new FollowRepository(ctx);
            var result = await repo.ToggleAsync(follower.Id, followed.Id);
            await ctx.SaveChangesAsync();

            result.Should().BeFalse();
            (await ctx.Follows.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task ToggleAsync_ShouldOnlyRemove_FollowMatchingBothIds()
        {
            using var fx = new DatabaseFixture();
            var (ctx, follower, followed) = await SeedAsync(fx);
            var otherFollower = new UserBuilder().Build();
            ctx.Users.Add(otherFollower);
            ctx.Follows.Add(Follow.Create(follower.Id, followed.Id));
            ctx.Follows.Add(Follow.Create(otherFollower.Id, followed.Id));
            await ctx.SaveChangesAsync();

            var repo = new FollowRepository(ctx);
            var result = await repo.ToggleAsync(follower.Id, followed.Id);
            await ctx.SaveChangesAsync();

            result.Should().BeFalse();
            (await ctx.Follows.CountAsync()).Should().Be(1);
            (await ctx.Follows.FirstAsync()).FollowerId.Should().Be(otherFollower.Id);
        }

        [Fact]
        public async Task ToggleAsync_ShouldAllowReFollow_AfterUnfollow()
        {
            using var fx = new DatabaseFixture();
            var (ctx, follower, followed) = await SeedAsync(fx);
            var repo = new FollowRepository(ctx);

            var followed1 = await repo.ToggleAsync(follower.Id, followed.Id);
            await ctx.SaveChangesAsync();
            var unfollowed = await repo.ToggleAsync(follower.Id, followed.Id);
            await ctx.SaveChangesAsync();
            var refollowed = await repo.ToggleAsync(follower.Id, followed.Id);
            await ctx.SaveChangesAsync();

            followed1.Should().BeTrue();
            unfollowed.Should().BeFalse();
            refollowed.Should().BeTrue();
            (await ctx.Follows.CountAsync()).Should().Be(1);
        }
    }
}
