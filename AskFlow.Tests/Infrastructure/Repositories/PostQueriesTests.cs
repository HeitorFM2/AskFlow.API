using AskFlow.Domain.Entities;
using AskFlow.Infrastructure.Repositories;
using AskFlow.Tests.Common.Builders;
using AskFlow.Tests.Common.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace AskFlow.Tests.Infrastructure.Repositories
{
    public class PostQueriesTests
    {
        [Fact]
        public async Task GetByIdAsync_ShouldReturn_NullWhenMissing()
        {
            using var fx = new DatabaseFixture();
            var queries = new PostQueries(fx.Context);

            var post = await queries.GetByIdAsync(99999);

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

            var queries = new PostQueries(ctx);
            var found = await queries.GetByIdAsync(post.Id);

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

            var queries = new PostQueries(ctx);
            var firstPage = await queries.GetAllAsync(1, 3);
            var secondPage = await queries.GetAllAsync(2, 3);
            var count = await queries.CountAsync();

            firstPage.Should().HaveCount(3);
            secondPage.Should().HaveCount(2);
            count.Should().Be(5);
            firstPage[0].User.UserName.Should().Be(user.UserName);
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

            var queries = new PostQueries(ctx);
            var found = await queries.GetByIdAsync(post.Id);

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
        public async Task GetByIdAsync_ShouldReturn_LikeCount_FromDenormalizedField()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var user = new UserBuilder().Build();
            ctx.Users.Add(user);
            var post = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            post.IncrementLikeCount();
            post.IncrementLikeCount();
            ctx.Posts.Add(post);
            await ctx.SaveChangesAsync();

            var queries = new PostQueries(ctx);
            var found = await queries.GetByIdAsync(post.Id);

            found.Should().NotBeNull();
            found!.Likes.Should().Be(2);
        }

        [Fact]
        public async Task GetAllAsync_ShouldProject_DenormalizedCounters()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var user = new UserBuilder().Build();
            ctx.Users.Add(user);
            var post = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            post.IncrementCommentCount();
            post.IncrementCommentCount();
            post.IncrementCommentCount();
            post.IncrementLikeCount();
            ctx.Posts.Add(post);
            await ctx.SaveChangesAsync();

            var queries = new PostQueries(ctx);
            var list = await queries.GetAllAsync(1, 10);

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

            var queries = new PostQueries(ctx);
            var list = await queries.GetAllAsync(1, 10);

            list.Select(p => p.Id).Should().Equal(new[] { p3.Id, p2.Id, p1.Id });
        }

        [Fact]
        public async Task CountAsync_ShouldReturn_Zero_OnEmptyDb()
        {
            using var fx = new DatabaseFixture();
            var queries = new PostQueries(fx.Context);

            (await queries.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task GetByUserAsync_ShouldReturn_Empty_WhenUserHasNoPosts()
        {
            using var fx = new DatabaseFixture();
            var queries = new PostQueries(fx.Context);

            var result = await queries.GetByUserAsync("user-1", 1, 10);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByUserAsync_ShouldReturn_OnlyPostsOfSpecifiedUser()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var user = new UserBuilder().Build();
            var otherUser = new UserBuilder().Build();
            ctx.Users.AddRange(user, otherUser);
            var myPost = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            var theirPost = new PostBuilder().WithId(0).WithUserId(otherUser.Id).Build();
            ctx.Posts.AddRange(myPost, theirPost);
            await ctx.SaveChangesAsync();

            var queries = new PostQueries(ctx);
            var result = await queries.GetByUserAsync(user.Id, 1, 10);

            result.Should().ContainSingle();
            result[0].Id.Should().Be(myPost.Id);
        }

        [Fact]
        public async Task GetByUserAsync_ShouldNotReturn_DeletedPosts()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var user = new UserBuilder().Build();
            ctx.Users.Add(user);
            var activePost = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            var deletedPost = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            ctx.Posts.AddRange(activePost, deletedPost);
            await ctx.SaveChangesAsync();
            deletedPost.MarkAsDeleted();
            await ctx.SaveChangesAsync();

            var queries = new PostQueries(ctx);
            var result = await queries.GetByUserAsync(user.Id, 1, 10);

            result.Should().ContainSingle();
            result[0].Id.Should().Be(activePost.Id);
        }

        [Fact]
        public async Task GetByUserAsync_ShouldOrder_ByCreatedAtDescending()
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

            var queries = new PostQueries(ctx);
            var result = await queries.GetByUserAsync(user.Id, 1, 10);

            result.Select(p => p.Id).Should().Equal(p2.Id, p1.Id);
        }

        [Fact]
        public async Task GetByUserAsync_ShouldRespect_Pagination()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var user = new UserBuilder().Build();
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            for (int i = 0; i < 5; i++)
            {
                ctx.Posts.Add(new PostBuilder().WithId(0).WithUserId(user.Id).Build());
                await ctx.SaveChangesAsync();
            }

            var queries = new PostQueries(ctx);
            var firstPage = await queries.GetByUserAsync(user.Id, 1, 3);
            var secondPage = await queries.GetByUserAsync(user.Id, 2, 3);

            firstPage.Should().HaveCount(3);
            secondPage.Should().HaveCount(2);
            firstPage.Select(p => p.Id).Should().NotIntersectWith(secondPage.Select(p => p.Id));
        }

        [Fact]
        public async Task GetByUserAsync_ShouldProject_AllFields_Correctly()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var user = new UserBuilder().Build();
            ctx.Users.Add(user);
            var post = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            post.IncrementCommentCount();
            post.IncrementLikeCount();
            ctx.Posts.Add(post);
            await ctx.SaveChangesAsync();

            var queries = new PostQueries(ctx);
            var result = await queries.GetByUserAsync(user.Id, 1, 10);

            result.Should().ContainSingle();
            var vm = result[0];
            vm.Id.Should().Be(post.Id);
            vm.Content.Should().Be(post.Content);
            vm.Comments.Should().Be(1);
            vm.Likes.Should().Be(1);
            vm.User.UserName.Should().Be(user.UserName);
            vm.User.Identification.Should().Be(user.Identification);
        }

        [Fact]
        public async Task CountByUserAsync_ShouldReturn_Zero_WhenUserHasNoPosts()
        {
            using var fx = new DatabaseFixture();
            var queries = new PostQueries(fx.Context);

            (await queries.CountByUserAsync("user-1")).Should().Be(0);
        }

        [Fact]
        public async Task CountByUserAsync_ShouldReturn_CorrectCount()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var user = new UserBuilder().Build();
            ctx.Users.Add(user);
            ctx.Posts.AddRange(
                new PostBuilder().WithId(0).WithUserId(user.Id).Build(),
                new PostBuilder().WithId(0).WithUserId(user.Id).Build());
            await ctx.SaveChangesAsync();

            var queries = new PostQueries(ctx);
            (await queries.CountByUserAsync(user.Id)).Should().Be(2);
        }

        [Fact]
        public async Task CountByUserAsync_ShouldNotCount_OtherUsersPosts()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var user = new UserBuilder().Build();
            var otherUser = new UserBuilder().Build();
            ctx.Users.AddRange(user, otherUser);
            ctx.Posts.Add(new PostBuilder().WithId(0).WithUserId(otherUser.Id).Build());
            await ctx.SaveChangesAsync();

            var queries = new PostQueries(ctx);
            (await queries.CountByUserAsync(user.Id)).Should().Be(0);
        }

        [Fact]
        public async Task CountByUserAsync_ShouldNotCount_DeletedPosts()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var user = new UserBuilder().Build();
            ctx.Users.Add(user);
            var activePost = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            var deletedPost = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            ctx.Posts.AddRange(activePost, deletedPost);
            await ctx.SaveChangesAsync();
            deletedPost.MarkAsDeleted();
            await ctx.SaveChangesAsync();

            var queries = new PostQueries(ctx);
            (await queries.CountByUserAsync(user.Id)).Should().Be(1);
        }

        [Fact]
        public async Task GetFollowingPostsAsync_ShouldReturn_OwnPosts()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var me = new UserBuilder().Build();
            ctx.Users.Add(me);
            var myPost = new PostBuilder().WithId(0).WithUserId(me.Id).Build();
            ctx.Posts.Add(myPost);
            await ctx.SaveChangesAsync();

            var queries = new PostQueries(ctx);
            var result = await queries.GetFollowingPostsAsync(me.Id, 1, 10);

            result.Should().ContainSingle();
            result[0].Id.Should().Be(myPost.Id);
        }

        [Fact]
        public async Task GetFollowingPostsAsync_ShouldReturn_PostsFromFollowedUsers()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var me = new UserBuilder().Build();
            var followed = new UserBuilder().Build();
            var stranger = new UserBuilder().Build();
            ctx.Users.AddRange(me, followed, stranger);
            ctx.Follows.Add(Follow.Create(me.Id, followed.Id));
            var followedPost = new PostBuilder().WithId(0).WithUserId(followed.Id).Build();
            var strangerPost = new PostBuilder().WithId(0).WithUserId(stranger.Id).Build();
            ctx.Posts.AddRange(followedPost, strangerPost);
            await ctx.SaveChangesAsync();

            var queries = new PostQueries(ctx);
            var result = await queries.GetFollowingPostsAsync(me.Id, 1, 10);

            result.Should().ContainSingle();
            result[0].Id.Should().Be(followedPost.Id);
        }

        [Fact]
        public async Task GetFollowingPostsAsync_ShouldReturn_OwnAndFollowedPosts_Together()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var me = new UserBuilder().Build();
            var followed = new UserBuilder().Build();
            ctx.Users.AddRange(me, followed);
            ctx.Follows.Add(Follow.Create(me.Id, followed.Id));
            var myPost = new PostBuilder().WithId(0).WithUserId(me.Id).Build();
            var followedPost = new PostBuilder().WithId(0).WithUserId(followed.Id).Build();
            ctx.Posts.AddRange(myPost, followedPost);
            await ctx.SaveChangesAsync();

            var queries = new PostQueries(ctx);
            var result = await queries.GetFollowingPostsAsync(me.Id, 1, 10);

            result.Should().HaveCount(2);
            result.Select(p => p.Id).Should().Contain([myPost.Id, followedPost.Id]);
        }

        [Fact]
        public async Task CountFollowingPostsAsync_ShouldCount_OwnAndFollowedPosts()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var me = new UserBuilder().Build();
            var followed = new UserBuilder().Build();
            var stranger = new UserBuilder().Build();
            ctx.Users.AddRange(me, followed, stranger);
            ctx.Follows.Add(Follow.Create(me.Id, followed.Id));
            ctx.Posts.AddRange(
                new PostBuilder().WithId(0).WithUserId(me.Id).Build(),
                new PostBuilder().WithId(0).WithUserId(followed.Id).Build(),
                new PostBuilder().WithId(0).WithUserId(stranger.Id).Build());
            await ctx.SaveChangesAsync();

            var queries = new PostQueries(ctx);
            (await queries.CountFollowingPostsAsync(me.Id)).Should().Be(2);
        }
    }
}
