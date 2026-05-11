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

        [Fact]
        public async Task GetByPostAsync_ShouldProject_ReplyCount_AndUser()
        {
            using var fx = new DatabaseFixture();
            var (ctx, postId, userId) = await SeedAsync(fx);
            var user = await ctx.Users.FirstAsync();
            var repo = new CommentRepository(ctx);

            var root = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).Build();
            await repo.AddAsync(root);
            await repo.AddAsync(new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).WithParentCommentId(root.Id).Build());
            await repo.AddAsync(new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).WithParentCommentId(root.Id).Build());

            var page = await repo.GetByPostAsync(postId, 1, 10);

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
            var repo = new CommentRepository(ctx);

            for (int i = 0; i < 5; i++)
            {
                await repo.AddAsync(new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).Build());
                await Task.Delay(5);
            }

            var firstPage = await repo.GetByPostAsync(postId, 1, 3);
            var secondPage = await repo.GetByPostAsync(postId, 2, 3);

            firstPage.Should().HaveCount(3);
            secondPage.Should().HaveCount(2);
            firstPage.Select(c => c.Id).Should().NotIntersectWith(secondPage.Select(c => c.Id));
        }

        [Fact]
        public async Task GetByPostAsync_ShouldReturn_Empty_WhenPostHasNoComments()
        {
            using var fx = new DatabaseFixture();
            var (ctx, postId, _) = await SeedAsync(fx);
            var repo = new CommentRepository(ctx);

            var page = await repo.GetByPostAsync(postId, 1, 10);

            page.Should().BeEmpty();
        }

        [Fact]
        public async Task GetRepliesAsync_ShouldProject_ReplyCount_OfNestedReplies()
        {
            using var fx = new DatabaseFixture();
            var (ctx, postId, userId) = await SeedAsync(fx);
            var user = await ctx.Users.FirstAsync();
            var repo = new CommentRepository(ctx);

            var root = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).Build();
            await repo.AddAsync(root);
            var reply = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).WithParentCommentId(root.Id).Build();
            await repo.AddAsync(reply);
            await repo.AddAsync(new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).WithParentCommentId(reply.Id).Build());
            await repo.AddAsync(new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).WithParentCommentId(reply.Id).Build());

            var replies = await repo.GetRepliesAsync(root.Id, 1, 10);

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
            var repo = new CommentRepository(ctx);

            var parent = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).Build();
            await repo.AddAsync(parent);
            for (int i = 0; i < 5; i++)
            {
                await repo.AddAsync(new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).WithParentCommentId(parent.Id).Build());
                await Task.Delay(5);
            }

            var firstPage = await repo.GetRepliesAsync(parent.Id, 1, 2);
            var secondPage = await repo.GetRepliesAsync(parent.Id, 2, 2);
            var thirdPage = await repo.GetRepliesAsync(parent.Id, 3, 2);

            firstPage.Should().HaveCount(2);
            secondPage.Should().HaveCount(2);
            thirdPage.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturn_Null_WhenMissing()
        {
            using var fx = new DatabaseFixture();
            var repo = new CommentRepository(fx.Context);

            var found = await repo.GetByIdAsync(99999);

            found.Should().BeNull();
        }

        [Fact]
        public async Task CountByPostAsync_ShouldReturn_Zero_WhenPostHasNoComments()
        {
            using var fx = new DatabaseFixture();
            var (ctx, postId, _) = await SeedAsync(fx);
            var repo = new CommentRepository(ctx);

            (await repo.CountByPostAsync(postId)).Should().Be(0);
        }

        [Fact]
        public async Task CountRepliesAsync_ShouldReturn_Zero_WhenParentHasNoReplies()
        {
            using var fx = new DatabaseFixture();
            var (ctx, postId, userId) = await SeedAsync(fx);
            var repo = new CommentRepository(ctx);

            var parent = new CommentBuilder().WithId(0).WithPostId(postId).WithUserId(userId).Build();
            await repo.AddAsync(parent);

            (await repo.CountRepliesAsync(parent.Id)).Should().Be(0);
        }
    }
}
