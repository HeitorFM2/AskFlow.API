using AskFlow.Infrastructure.Repositories;
using AskFlow.Tests.Common.Builders;
using AskFlow.Tests.Common.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace AskFlow.Tests.Infrastructure.Repositories
{
    public class PostRepositoryTests
    {
        [Fact]
        public async Task AddAsync_ShouldPersist_NewPost()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var repo = new PostRepository(ctx);
            var user = new UserBuilder().Build();
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            var post = new PostBuilder().WithId(0).WithUserId(user.Id).Build();

            await repo.AddAsync(post);

            post.Id.Should().BeGreaterThan(0);
            (await ctx.Posts.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturn_NullWhenMissing()
        {
            using var fx = new DatabaseFixture();
            var repo = new PostRepository(fx.Context);

            var post = await repo.GetByIdAsync(99999);

            post.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturn_PostWithRelations()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var user = new UserBuilder().Build();
            ctx.Users.Add(user);
            var post = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            ctx.Posts.Add(post);
            await ctx.SaveChangesAsync();

            var repo = new PostRepository(ctx);
            var found = await repo.GetByIdAsync(post.Id);

            found.Should().NotBeNull();
            found!.User.UserName.Should().Be(user.UserName);
        }

        [Fact]
        public async Task GetAllAsync_ShouldRespectPagination_AndIncludeUser()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var user = new UserBuilder().Build();
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            var posts = Enumerable.Range(1, 5)
                .Select(_ => new PostBuilder().WithId(0).WithUserId(user.Id).Build())
                .ToList();
            ctx.Posts.AddRange(posts);
            await ctx.SaveChangesAsync();

            var repo = new PostRepository(ctx);
            var firstPage = await repo.GetAllAsync(1, 3);
            var secondPage = await repo.GetAllAsync(2, 3);
            var count = await repo.CountAsync();

            firstPage.Should().HaveCount(3);
            secondPage.Should().HaveCount(2);
            count.Should().Be(5);
            firstPage[0].User.UserName.Should().Be(user.UserName);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemove_FromContext()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var user = new UserBuilder().Build();
            ctx.Users.Add(user);
            var post = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            ctx.Posts.Add(post);
            await ctx.SaveChangesAsync();

            var repo = new PostRepository(ctx);
            await repo.DeleteAsync(post);

            (await ctx.Posts.IgnoreQueryFilters().CountAsync()).Should().Be(0);
        }
    }
}
