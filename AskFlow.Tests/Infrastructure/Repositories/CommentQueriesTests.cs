using AskFlow.Infrastructure.Repositories;
using AskFlow.Tests.Common.Builders;
using AskFlow.Tests.Common.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace AskFlow.Tests.Infrastructure.Repositories
{
    public class CommentQueriesTests
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
        public async Task GetByPostAsync_ShouldReturn_OnlyRootComments_OrderedDesc()
        {
            using var fx = new DatabaseFixture();
            var (ctx, postId, userId) = await SeedAsync(fx);

            var older = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).Build();
            ctx.Comments.Add(older);
            await ctx.SaveChangesAsync();
            await Task.Delay(10);
            var newer = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).Build();
            ctx.Comments.Add(newer);
            await ctx.SaveChangesAsync();
            ctx.Comments.Add(new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).WithParentCommentId(older.Id).Build());
            await ctx.SaveChangesAsync();

            var queries = new CommentQueries(ctx);
            var page = await queries.GetByPostAsync(postId, 1, 10);

            page.Should().HaveCount(2);
            page[0].Id.Should().Be(newer.Id);
            page[1].Id.Should().Be(older.Id);
        }

        [Fact]
        public async Task GetRepliesAsync_ShouldReturn_OnlyChildren_OrderedAsc()
        {
            using var fx = new DatabaseFixture();
            var (ctx, postId, userId) = await SeedAsync(fx);

            var parent = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).Build();
            ctx.Comments.Add(parent);
            await ctx.SaveChangesAsync();
            var r1 = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).WithParentCommentId(parent.Id).Build();
            ctx.Comments.Add(r1);
            await ctx.SaveChangesAsync();
            await Task.Delay(10);
            var r2 = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).WithParentCommentId(parent.Id).Build();
            ctx.Comments.Add(r2);
            await ctx.SaveChangesAsync();

            var queries = new CommentQueries(ctx);
            var replies = await queries.GetRepliesAsync(parent.Id, 1, 10);

            replies.Select(r => r.Id).Should().Equal(new[] { r1.Id, r2.Id });
        }

        [Fact]
        public async Task CountByPostAsync_ShouldOnlyCount_RootComments()
        {
            using var fx = new DatabaseFixture();
            var (ctx, postId, userId) = await SeedAsync(fx);

            var p1 = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).Build();
            ctx.Comments.Add(p1);
            ctx.Comments.Add(new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).Build());
            await ctx.SaveChangesAsync();
            ctx.Comments.Add(new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).WithParentCommentId(p1.Id).Build());
            await ctx.SaveChangesAsync();

            var queries = new CommentQueries(ctx);
            (await queries.CountByPostAsync(postId)).Should().Be(2);
        }

        [Fact]
        public async Task CountRepliesAsync_ShouldCount_ChildrenOnly()
        {
            using var fx = new DatabaseFixture();
            var (ctx, postId, userId) = await SeedAsync(fx);

            var parent = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).Build();
            ctx.Comments.Add(parent);
            await ctx.SaveChangesAsync();
            for (int i = 0; i < 3; i++)
                ctx.Comments.Add(new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).WithParentCommentId(parent.Id).Build());
            await ctx.SaveChangesAsync();

            var queries = new CommentQueries(ctx);
            (await queries.CountRepliesAsync(parent.Id)).Should().Be(3);
        }

        [Fact]
        public async Task GetByPostAsync_ShouldProject_ReplyCount_AndUser()
        {
            using var fx = new DatabaseFixture();
            var (ctx, postId, userId) = await SeedAsync(fx);
            var user = await ctx.Users.FirstAsync();

            var root = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).Build();
            ctx.Comments.Add(root);
            await ctx.SaveChangesAsync();
            ctx.Comments.Add(new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).WithParentCommentId(root.Id).Build());
            ctx.Comments.Add(new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).WithParentCommentId(root.Id).Build());
            await ctx.SaveChangesAsync();

            var queries = new CommentQueries(ctx);
            var page = await queries.GetByPostAsync(postId, 1, 10);

            page.Should().ContainSingle();
            page[0].Id.Should().Be(root.Id);
            page[0].Content.Should().Be(root.Content);
            page[0].ParentCommentId.Should().BeNull();
            page[0].ReplyCount.Should().Be(2);
            page[0].User.UserName.Should().Be(user.UserName);
            page[0].User.Identification.Should().Be(user.Identification);
        }

        [Fact]
        public async Task GetByPostAsync_ShouldRespect_Pagination()
        {
            using var fx = new DatabaseFixture();
            var (ctx, postId, userId) = await SeedAsync(fx);

            for (int i = 0; i < 5; i++)
            {
                ctx.Comments.Add(new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).Build());
                await ctx.SaveChangesAsync();
                await Task.Delay(5);
            }

            var queries = new CommentQueries(ctx);
            var firstPage = await queries.GetByPostAsync(postId, 1, 3);
            var secondPage = await queries.GetByPostAsync(postId, 2, 3);

            firstPage.Should().HaveCount(3);
            secondPage.Should().HaveCount(2);
            firstPage.Select(c => c.Id).Should().NotIntersectWith(secondPage.Select(c => c.Id));
        }

        [Fact]
        public async Task GetByPostAsync_ShouldReturn_Empty_WhenPostHasNoComments()
        {
            using var fx = new DatabaseFixture();
            var (ctx, postId, _) = await SeedAsync(fx);

            var queries = new CommentQueries(ctx);
            var page = await queries.GetByPostAsync(postId, 1, 10);

            page.Should().BeEmpty();
        }

        [Fact]
        public async Task GetRepliesAsync_ShouldProject_ReplyCount_OfNestedReplies()
        {
            using var fx = new DatabaseFixture();
            var (ctx, postId, userId) = await SeedAsync(fx);
            var user = await ctx.Users.FirstAsync();

            var root = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).Build();
            ctx.Comments.Add(root);
            await ctx.SaveChangesAsync();
            var reply = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).WithParentCommentId(root.Id).Build();
            ctx.Comments.Add(reply);
            await ctx.SaveChangesAsync();
            ctx.Comments.Add(new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).WithParentCommentId(reply.Id).Build());
            ctx.Comments.Add(new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).WithParentCommentId(reply.Id).Build());
            await ctx.SaveChangesAsync();

            var queries = new CommentQueries(ctx);
            var replies = await queries.GetRepliesAsync(root.Id, 1, 10);

            replies.Should().ContainSingle();
            replies[0].Id.Should().Be(reply.Id);
            replies[0].ParentCommentId.Should().Be(root.Id);
            replies[0].ReplyCount.Should().Be(2);
            replies[0].Content.Should().Be(reply.Content);
            replies[0].User.UserName.Should().Be(user.UserName);
            replies[0].User.Identification.Should().Be(user.Identification);
        }

        [Fact]
        public async Task GetRepliesAsync_ShouldRespect_Pagination()
        {
            using var fx = new DatabaseFixture();
            var (ctx, postId, userId) = await SeedAsync(fx);

            var parent = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).Build();
            ctx.Comments.Add(parent);
            await ctx.SaveChangesAsync();
            for (int i = 0; i < 5; i++)
            {
                ctx.Comments.Add(new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).WithParentCommentId(parent.Id).Build());
                await ctx.SaveChangesAsync();
                await Task.Delay(5);
            }

            var queries = new CommentQueries(ctx);
            var firstPage = await queries.GetRepliesAsync(parent.Id, 1, 2);
            var secondPage = await queries.GetRepliesAsync(parent.Id, 2, 2);
            var thirdPage = await queries.GetRepliesAsync(parent.Id, 3, 2);

            firstPage.Should().HaveCount(2);
            secondPage.Should().HaveCount(2);
            thirdPage.Should().HaveCount(1);
        }

        [Fact]
        public async Task CountByPostAsync_ShouldReturn_Zero_WhenPostHasNoComments()
        {
            using var fx = new DatabaseFixture();
            var (ctx, postId, _) = await SeedAsync(fx);

            var queries = new CommentQueries(ctx);
            (await queries.CountByPostAsync(postId)).Should().Be(0);
        }

        [Fact]
        public async Task CountRepliesAsync_ShouldReturn_Zero_WhenParentHasNoReplies()
        {
            using var fx = new DatabaseFixture();
            var (ctx, postId, userId) = await SeedAsync(fx);

            var parent = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).Build();
            ctx.Comments.Add(parent);
            await ctx.SaveChangesAsync();

            var queries = new CommentQueries(ctx);
            (await queries.CountRepliesAsync(parent.Id)).Should().Be(0);
        }
    }
}
