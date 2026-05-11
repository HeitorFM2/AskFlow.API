using AskFlow.Domain.Entities;
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
        public async Task DeleteAsync_ShouldSoftDelete()
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

            (await ctx.Posts.CountAsync()).Should().Be(0);
            var stored = await ctx.Posts.IgnoreQueryFilters().SingleAsync();
            stored.IsDeleted.Should().BeTrue();
            stored.DeletedAt.Should().NotBeNull();
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
        public async Task GetByIdAsync_ShouldProject_RootComments_WithReplyCount_OrderedDesc()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var user = new UserBuilder().Build();
            ctx.Users.Add(user);
            var post = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            ctx.Posts.Add(post);
            await ctx.SaveChangesAsync();

            var olderRoot = new CommentBuilder().WithId(0).WithPostId(post.Id).WithUserId(user.Id).Build();
            ctx.Comments.Add(olderRoot);
            await ctx.SaveChangesAsync();
            await Task.Delay(10);
            var newerRoot = new CommentBuilder().WithId(0).WithPostId(post.Id).WithUserId(user.Id).Build();
            ctx.Comments.Add(newerRoot);
            await ctx.SaveChangesAsync();
            var reply1 = new CommentBuilder().WithId(0).WithPostId(post.Id).WithUserId(user.Id).WithParentCommentId(olderRoot.Id).Build();
            var reply2 = new CommentBuilder().WithId(0).WithPostId(post.Id).WithUserId(user.Id).WithParentCommentId(olderRoot.Id).Build();
            ctx.Comments.AddRange(reply1, reply2);
            await ctx.SaveChangesAsync();

            var repo = new PostRepository(ctx);
            var found = await repo.GetByIdAsync(post.Id);

            found.Should().NotBeNull();
            found!.Id.Should().Be(post.Id);
            found.Content.Should().Be(post.Content);
            var comments = found.Comments.ToList();
            comments.Should().HaveCount(2);
            comments[0].Id.Should().Be(newerRoot.Id);
            comments[0].ReplyCount.Should().Be(0);
            comments[0].User.UserName.Should().Be(user.UserName);
            comments[0].User.Identification.Should().Be(user.Identification);
            comments[1].Id.Should().Be(olderRoot.Id);
            comments[1].ReplyCount.Should().Be(2);
            comments[1].ParentCommentId.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturn_LikesCount()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var user = new UserBuilder().Build();
            var otherUser = new UserBuilder().Build();
            ctx.Users.AddRange(user, otherUser);
            var post = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            ctx.Posts.Add(post);
            await ctx.SaveChangesAsync();

            ctx.Likes.AddRange(
                new Like(post.Id, user.Id) { User = user, Post = post },
                new Like(post.Id, otherUser.Id) { User = otherUser, Post = post });
            await ctx.SaveChangesAsync();

            var repo = new PostRepository(ctx);
            var found = await repo.GetByIdAsync(post.Id);

            found.Should().NotBeNull();
            found!.Likes.Should().Be(2);
        }

        [Fact]
        public async Task GetAllAsync_ShouldProject_CommentsAndLikes_Counts()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var user = new UserBuilder().Build();
            ctx.Users.Add(user);
            var post = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            ctx.Posts.Add(post);
            await ctx.SaveChangesAsync();

            ctx.Comments.AddRange(
                new CommentBuilder().WithId(0).WithPostId(post.Id).WithUserId(user.Id).Build(),
                new CommentBuilder().WithId(0).WithPostId(post.Id).WithUserId(user.Id).Build(),
                new CommentBuilder().WithId(0).WithPostId(post.Id).WithUserId(user.Id).Build());
            ctx.Likes.Add(new Like(post.Id, user.Id) { User = user, Post = post });
            await ctx.SaveChangesAsync();

            var repo = new PostRepository(ctx);
            var list = await repo.GetAllAsync(1, 10);

            list.Should().ContainSingle();
            list[0].Id.Should().Be(post.Id);
            list[0].Content.Should().Be(post.Content);
            list[0].Comments.Should().Be(3);
            list[0].Likes.Should().Be(1);
            list[0].User.Identification.Should().Be(user.Identification);
        }

        [Fact]
        public async Task GetAllAsync_ShouldOrder_ByCreatedAtDescending()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var user = new UserBuilder().Build();
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            var p1 = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            ctx.Posts.Add(p1);
            await ctx.SaveChangesAsync();
            await Task.Delay(10);
            var p2 = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            ctx.Posts.Add(p2);
            await ctx.SaveChangesAsync();
            await Task.Delay(10);
            var p3 = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            ctx.Posts.Add(p3);
            await ctx.SaveChangesAsync();

            var repo = new PostRepository(ctx);
            var list = await repo.GetAllAsync(1, 10);

            list.Select(p => p.Id).Should().Equal(new[] { p3.Id, p2.Id, p1.Id });
        }

        [Fact]
        public async Task CountAsync_ShouldReturn_Zero_OnEmptyDb()
        {
            using var fx = new DatabaseFixture();
            var repo = new PostRepository(fx.Context);

            (await repo.CountAsync()).Should().Be(0);
        }
    }
}
