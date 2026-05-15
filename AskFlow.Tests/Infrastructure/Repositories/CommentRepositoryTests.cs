using AskFlow.Infrastructure.Repositories;
using AskFlow.Tests.Common.Builders;
using AskFlow.Tests.Common.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace AskFlow.Tests.Infrastructure.Repositories
{
    public class CommentRepositoryTests
    {
        private static async Task<(AskFlow.Infrastructure.Data.AppDbContext ctx, int postId, string userId)> SeedAsync(DatabaseFixture fx)
        {
            var ctx = fx.Context;
            var user = new UserBuilder().Build();
            ctx.Users.Add(user);
            var post = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            ctx.Posts.Add(post);
            await ctx.SaveChangesAsync();
            return (ctx, post.Id, user.Id);
        }

        [Fact]
        public async Task Add_AndFindByIdAsync_ShouldRoundtrip()
        {
            using var fx = new DatabaseFixture();
            var (ctx, postId, userId) = await SeedAsync(fx);
            var repo = new CommentRepository(ctx);

            var comment = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).Build();
            repo.Add(comment);
            await ctx.SaveChangesAsync();

            var fetched = await repo.FindByIdAsync(comment.Id);
            fetched.Should().NotBeNull();
            fetched!.UserId.Should().Be(userId);
        }

        [Fact]
        public async Task FindByIdAsync_ShouldReturn_Null_WhenMissing()
        {
            using var fx = new DatabaseFixture();
            var repo = new CommentRepository(fx.Context);

            var found = await repo.FindByIdAsync(99999);

            found.Should().BeNull();
        }

        [Fact]
        public async Task Delete_ShouldRemoveComment_AfterSave()
        {
            using var fx = new DatabaseFixture();
            var (ctx, postId, userId) = await SeedAsync(fx);
            var repo = new CommentRepository(ctx);

            var c = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).Build();
            repo.Add(c);
            await ctx.SaveChangesAsync();

            repo.Delete(c);
            await ctx.SaveChangesAsync();

            (await ctx.Comments.CountAsync()).Should().Be(0);
        }
    }
}
