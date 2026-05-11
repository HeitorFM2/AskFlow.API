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
        public async Task AddAsync_GetByIdAsync_ShouldRoundtrip()
        {
            using var fx = new DatabaseFixture();
            var (ctx, postId, userId) = await SeedAsync(fx);
            var repo = new CommentRepository(ctx);

            var comment = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).Build();
            await repo.AddAsync(comment);

            var fetched = await repo.GetByIdAsync(comment.Id);
            fetched.Should().NotBeNull();
            fetched!.UserId.Should().Be(userId);
        }

        [Fact]
        public async Task GetByPostAsync_ShouldReturn_OnlyRootComments_OrderedDesc()
        {
            using var fx = new DatabaseFixture();
            var (ctx, postId, userId) = await SeedAsync(fx);
            var repo = new CommentRepository(ctx);

            var older = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).Build();
            await repo.AddAsync(older);
            await Task.Delay(10);
            var newer = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).Build();
            await repo.AddAsync(newer);
            var reply = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).WithParentCommentId(older.Id).Build();
            await repo.AddAsync(reply);

            var page = await repo.GetByPostAsync(postId, 1, 10);

            page.Should().HaveCount(2);
            page[0].Id.Should().Be(newer.Id);
            page[1].Id.Should().Be(older.Id);
        }

        [Fact]
        public async Task GetRepliesAsync_ShouldReturn_OnlyChildren_OrderedAsc()
        {
            using var fx = new DatabaseFixture();
            var (ctx, postId, userId) = await SeedAsync(fx);
            var repo = new CommentRepository(ctx);

            var parent = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).Build();
            await repo.AddAsync(parent);
            var r1 = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).WithParentCommentId(parent.Id).Build();
            await repo.AddAsync(r1);
            await Task.Delay(10);
            var r2 = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).WithParentCommentId(parent.Id).Build();
            await repo.AddAsync(r2);

            var replies = await repo.GetRepliesAsync(parent.Id, 1, 10);

            replies.Select(r => r.Id).Should().Equal(new[] { r1.Id, r2.Id });
        }

        [Fact]
        public async Task CountByPostAsync_ShouldOnlyCount_RootComments()
        {
            using var fx = new DatabaseFixture();
            var (ctx, postId, userId) = await SeedAsync(fx);
            var repo = new CommentRepository(ctx);

            var p1 = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).Build();
            await repo.AddAsync(p1);
            var p2 = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).Build();
            await repo.AddAsync(p2);
            var reply = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).WithParentCommentId(p1.Id).Build();
            await repo.AddAsync(reply);

            (await repo.CountByPostAsync(postId)).Should().Be(2);
        }

        [Fact]
        public async Task CountRepliesAsync_ShouldCount_ChildrenOnly()
        {
            using var fx = new DatabaseFixture();
            var (ctx, postId, userId) = await SeedAsync(fx);
            var repo = new CommentRepository(ctx);

            var parent = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).Build();
            await repo.AddAsync(parent);
            for (int i = 0; i < 3; i++)
                await repo.AddAsync(new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).WithParentCommentId(parent.Id).Build());

            (await repo.CountRepliesAsync(parent.Id)).Should().Be(3);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveComment()
        {
            using var fx = new DatabaseFixture();
            var (ctx, postId, userId) = await SeedAsync(fx);
            var repo = new CommentRepository(ctx);

            var c = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).Build();
            await repo.AddAsync(c);

            await repo.DeleteAsync(c);

            (await ctx.Comments.CountAsync()).Should().Be(0);
        }
    }
}
