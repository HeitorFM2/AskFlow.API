using AskFlow.Infrastructure.Repositories;
using AskFlow.Tests.Common.Builders;
using AskFlow.Tests.Common.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace AskFlow.Tests.Infrastructure.Repositories
{
    public class PostRepositoryTests
    {
        [Fact]
        public async Task Add_ShouldStage_AndPersistAfterSave()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var repo = new PostRepository(ctx);
            var user = new UserBuilder().Build();
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            var post = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            repo.Add(post);
            await ctx.SaveChangesAsync();

            post.Id.Should().BeGreaterThan(0);
            (await ctx.Posts.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task FindByIdAsync_ShouldReturn_PostEntity_WhenFound()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var user = new UserBuilder().Build();
            ctx.Users.Add(user);
            var post = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            ctx.Posts.Add(post);
            await ctx.SaveChangesAsync();

            var repo = new PostRepository(ctx);
            var found = await repo.FindByIdAsync(post.Id);

            found.Should().NotBeNull();
            found!.Id.Should().Be(post.Id);
            found.UserId.Should().Be(user.Id);
        }

        [Fact]
        public async Task FindByIdAsync_ShouldReturn_Null_WhenMissing()
        {
            using var fx = new DatabaseFixture();
            var repo = new PostRepository(fx.Context);

            var found = await repo.FindByIdAsync(99999);

            found.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ShouldSoftDelete_AfterSave()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var user = new UserBuilder().Build();
            ctx.Users.Add(user);
            var post = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            ctx.Posts.Add(post);
            await ctx.SaveChangesAsync();

            var repo = new PostRepository(ctx);
            repo.Delete(post);
            await ctx.SaveChangesAsync();

            (await ctx.Posts.CountAsync()).Should().Be(0);
            var stored = await ctx.Posts.IgnoreQueryFilters().SingleAsync();
            stored.IsDeleted.Should().BeTrue();
            stored.DeletedAt.Should().NotBeNull();
        }
    }
}
