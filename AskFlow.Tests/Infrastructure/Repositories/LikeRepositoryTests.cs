using AskFlow.Domain.Entities;
using AskFlow.Infrastructure.Repositories;
using AskFlow.Tests.Common.Builders;
using AskFlow.Tests.Common.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace AskFlow.Tests.Infrastructure.Repositories
{
    public class LikeRepositoryTests
    {
        private static async Task<(AskFlow.Infrastructure.Data.AppDbContext ctx, User user, Post post)> SeedAsync(DatabaseFixture fx)
        {
            var ctx = fx.Context;
            var user = new UserBuilder().Build();
            ctx.Users.Add(user);
            var post = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            ctx.Posts.Add(post);
            await ctx.SaveChangesAsync();
            return (ctx, user, post);
        }

        [Fact]
        public async Task ToggleAsync_ShouldAddLike_AndReturnTrue_WhenNotAlreadyLiked()
        {
            using var fx = new DatabaseFixture();
            var (ctx, user, post) = await SeedAsync(fx);
            var repo = new LikeRepository(ctx);

            var result = await repo.ToggleAsync(post, user.Id);
            await ctx.SaveChangesAsync();

            result.Should().BeTrue();
            (await ctx.Likes.CountAsync()).Should().Be(1);
            post.LikeCount.Should().Be(1);
        }

        [Fact]
        public async Task ToggleAsync_ShouldRemoveLike_AndReturnFalse_WhenAlreadyLiked()
        {
            using var fx = new DatabaseFixture();
            var (ctx, user, post) = await SeedAsync(fx);
            ctx.Likes.Add(new Like(post.Id, user.Id) { User = user, Post = post });
            post.IncrementLikeCount();
            await ctx.SaveChangesAsync();

            var repo = new LikeRepository(ctx);
            var result = await repo.ToggleAsync(post, user.Id);
            await ctx.SaveChangesAsync();

            result.Should().BeFalse();
            (await ctx.Likes.CountAsync()).Should().Be(0);
            post.LikeCount.Should().Be(0);
        }

        [Fact]
        public async Task ToggleAsync_ShouldOnlyRemove_LikeMatchingUserAndPost()
        {
            using var fx = new DatabaseFixture();
            var (ctx, user, post) = await SeedAsync(fx);
            var otherUser = new UserBuilder().Build();
            ctx.Users.Add(otherUser);
            await ctx.SaveChangesAsync();

            ctx.Likes.AddRange(
                new Like(post.Id, user.Id) { User = user, Post = post },
                new Like(post.Id, otherUser.Id) { User = otherUser, Post = post });
            post.IncrementLikeCount();
            post.IncrementLikeCount();
            await ctx.SaveChangesAsync();

            var repo = new LikeRepository(ctx);
            var result = await repo.ToggleAsync(post, user.Id);
            await ctx.SaveChangesAsync();

            result.Should().BeFalse();
            (await ctx.Likes.CountAsync()).Should().Be(1);
            (await ctx.Likes.FirstAsync()).UserId.Should().Be(otherUser.Id);
            post.LikeCount.Should().Be(1);
        }

        [Fact]
        public async Task ToggleAsync_ShouldAllowReLike_AfterUnlike()
        {
            using var fx = new DatabaseFixture();
            var (ctx, user, post) = await SeedAsync(fx);
            var repo = new LikeRepository(ctx);

            var liked = await repo.ToggleAsync(post, user.Id);
            await ctx.SaveChangesAsync();
            var unliked = await repo.ToggleAsync(post, user.Id);
            await ctx.SaveChangesAsync();
            var reliked = await repo.ToggleAsync(post, user.Id);
            await ctx.SaveChangesAsync();

            liked.Should().BeTrue();
            unliked.Should().BeFalse();
            reliked.Should().BeTrue();
            (await ctx.Likes.CountAsync()).Should().Be(1);
            post.LikeCount.Should().Be(1);
        }
    }
}
