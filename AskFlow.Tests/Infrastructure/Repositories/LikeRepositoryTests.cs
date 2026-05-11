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

        // GetLikePostsAsync

        [Fact]
        public async Task GetLikePostsAsync_ShouldReturn_Empty_WhenUserHasNoLikes()
        {
            using var fx = new DatabaseFixture();
            var (ctx, user, _) = await SeedAsync(fx);
            var repo = new LikeRepository(ctx);

            var result = await repo.GetLikePostsAsync(user.Id, 1, 10);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetLikePostsAsync_ShouldReturn_OnlyPostsLikedByTheSpecifiedUser()
        {
            using var fx = new DatabaseFixture();
            var (ctx, user, post) = await SeedAsync(fx);
            var otherUser = new UserBuilder().Build();
            ctx.Users.Add(otherUser);
            var otherPost = new PostBuilder().WithId(0).WithUserId(otherUser.Id).Build();
            ctx.Posts.Add(otherPost);
            await ctx.SaveChangesAsync();

            ctx.Likes.Add(new Like(post.Id, user.Id) { User = user, Post = post });
            ctx.Likes.Add(new Like(otherPost.Id, otherUser.Id) { User = otherUser, Post = otherPost });
            await ctx.SaveChangesAsync();

            var repo = new LikeRepository(ctx);
            var result = await repo.GetLikePostsAsync(user.Id, 1, 10);

            result.Should().ContainSingle();
            result[0].PostId.Should().Be(post.Id);
        }

        [Fact]
        public async Task GetLikePostsAsync_ShouldProject_AllFields_Correctly()
        {
            using var fx = new DatabaseFixture();
            var (ctx, user, post) = await SeedAsync(fx);

            ctx.Comments.Add(new CommentBuilder().WithId(0).WithPostId(post.Id).WithUserId(user.Id).Build());
            ctx.Likes.Add(new Like(post.Id, user.Id) { User = user, Post = post });
            await ctx.SaveChangesAsync();

            var repo = new LikeRepository(ctx);
            var result = await repo.GetLikePostsAsync(user.Id, 1, 10);

            result.Should().ContainSingle();
            var dto = result[0];
            dto.PostId.Should().Be(post.Id);
            dto.Content.Should().Be(post.Content);
            dto.CreatedAt.Should().Be(post.CreatedAt);
            dto.CommentsCount.Should().Be(1);
            dto.LikesCount.Should().Be(1);
            dto.AuthorUserName.Should().Be(user.UserName);
            dto.AuthorIdentification.Should().Be(user.Identification);
        }

        [Fact]
        public async Task GetLikePostsAsync_ShouldOrder_ByPostCreatedAt_Descending()
        {
            using var fx = new DatabaseFixture();
            var (ctx, user, _) = await SeedAsync(fx);

            var older = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            ctx.Posts.Add(older);
            await ctx.SaveChangesAsync();
            await Task.Delay(10);
            var newer = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            ctx.Posts.Add(newer);
            await ctx.SaveChangesAsync();

            ctx.Likes.AddRange(
                new Like(older.Id, user.Id) { User = user, Post = older },
                new Like(newer.Id, user.Id) { User = user, Post = newer });
            await ctx.SaveChangesAsync();

            var repo = new LikeRepository(ctx);
            var result = await repo.GetLikePostsAsync(user.Id, 1, 10);

            result.Select(p => p.PostId).Should().Equal(newer.Id, older.Id);
        }

        [Fact]
        public async Task GetLikePostsAsync_ShouldRespect_Pagination()
        {
            using var fx = new DatabaseFixture();
            var (ctx, user, _) = await SeedAsync(fx);

            for (int i = 0; i < 5; i++)
            {
                var p = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
                ctx.Posts.Add(p);
                await ctx.SaveChangesAsync();
                ctx.Likes.Add(new Like(p.Id, user.Id) { User = user, Post = p });
                await ctx.SaveChangesAsync();
                await Task.Delay(5);
            }

            var repo = new LikeRepository(ctx);
            var firstPage = await repo.GetLikePostsAsync(user.Id, 1, 3);
            var secondPage = await repo.GetLikePostsAsync(user.Id, 2, 3);

            firstPage.Should().HaveCount(3);
            secondPage.Should().HaveCount(2);
            firstPage.Select(p => p.PostId).Should().NotIntersectWith(secondPage.Select(p => p.PostId));
        }

        // CountLikedPostAsync

        [Fact]
        public async Task CountLikedPostAsync_ShouldReturn_Zero_WhenUserHasNoLikes()
        {
            using var fx = new DatabaseFixture();
            var (ctx, user, _) = await SeedAsync(fx);
            var repo = new LikeRepository(ctx);

            var count = await repo.CountLikedPostAsync(user.Id);

            count.Should().Be(0);
        }

        [Fact]
        public async Task CountLikedPostAsync_ShouldReturn_CorrectCount_ForUser()
        {
            using var fx = new DatabaseFixture();
            var (ctx, user, post) = await SeedAsync(fx);
            var post2 = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            ctx.Posts.Add(post2);
            await ctx.SaveChangesAsync();

            ctx.Likes.AddRange(
                new Like(post.Id, user.Id) { User = user, Post = post },
                new Like(post2.Id, user.Id) { User = user, Post = post2 });
            await ctx.SaveChangesAsync();

            var repo = new LikeRepository(ctx);
            var count = await repo.CountLikedPostAsync(user.Id);

            count.Should().Be(2);
        }

        [Fact]
        public async Task CountLikedPostAsync_ShouldNotCount_OtherUsersLikes()
        {
            using var fx = new DatabaseFixture();
            var (ctx, user, post) = await SeedAsync(fx);
            var otherUser = new UserBuilder().Build();
            ctx.Users.Add(otherUser);
            await ctx.SaveChangesAsync();

            ctx.Likes.Add(new Like(post.Id, otherUser.Id) { User = otherUser, Post = post });
            await ctx.SaveChangesAsync();

            var repo = new LikeRepository(ctx);
            var count = await repo.CountLikedPostAsync(user.Id);

            count.Should().Be(0);
        }

        // GetLikedPostIdsAsync

        [Fact]
        public async Task GetLikedPostIdsAsync_ShouldReturn_EmptyHashSet_WhenPostIdsIsEmpty()
        {
            using var fx = new DatabaseFixture();
            var (ctx, user, _) = await SeedAsync(fx);
            var repo = new LikeRepository(ctx);

            var result = await repo.GetLikedPostIdsAsync(user.Id, []);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetLikedPostIdsAsync_ShouldReturn_OnlyLikedPostIds_FromTheGivenList()
        {
            using var fx = new DatabaseFixture();
            var (ctx, user, post) = await SeedAsync(fx);
            var post2 = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            var post3 = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            ctx.Posts.AddRange(post2, post3);
            await ctx.SaveChangesAsync();

            ctx.Likes.AddRange(
                new Like(post.Id, user.Id) { User = user, Post = post },
                new Like(post2.Id, user.Id) { User = user, Post = post2 });
            await ctx.SaveChangesAsync();

            var repo = new LikeRepository(ctx);
            var result = await repo.GetLikedPostIdsAsync(user.Id, [post.Id, post2.Id, post3.Id]);

            result.Should().BeEquivalentTo(new[] { post.Id, post2.Id });
        }

        [Fact]
        public async Task GetLikedPostIdsAsync_ShouldReturn_Empty_WhenNoneOfThePostsAreLiked()
        {
            using var fx = new DatabaseFixture();
            var (ctx, user, post) = await SeedAsync(fx);
            var repo = new LikeRepository(ctx);

            var result = await repo.GetLikedPostIdsAsync(user.Id, [post.Id]);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetLikedPostIdsAsync_ShouldNotReturn_PostsLikedByOtherUsers()
        {
            using var fx = new DatabaseFixture();
            var (ctx, user, post) = await SeedAsync(fx);
            var otherUser = new UserBuilder().Build();
            ctx.Users.Add(otherUser);
            await ctx.SaveChangesAsync();

            ctx.Likes.Add(new Like(post.Id, otherUser.Id) { User = otherUser, Post = post });
            await ctx.SaveChangesAsync();

            var repo = new LikeRepository(ctx);
            var result = await repo.GetLikedPostIdsAsync(user.Id, [post.Id]);

            result.Should().BeEmpty();
        }

        // ToggleLikeAsync
        // Uses SqliteDatabaseFixture because ExecuteDeleteAsync is not supported by the InMemory provider.

        private static async Task<(AskFlow.Infrastructure.Data.AppDbContext ctx, User user, Post post)> SeedSqliteAsync(SqliteDatabaseFixture fx)
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
        public async Task ToggleLikeAsync_ShouldAddLike_AndReturnTrue_WhenNotAlreadyLiked()
        {
            using var fx = new SqliteDatabaseFixture();
            var (ctx, user, post) = await SeedSqliteAsync(fx);
            var repo = new LikeRepository(ctx);

            var result = await repo.ToggleLikeAsync(user.Id, post.Id);

            result.Should().BeTrue();
            (await ctx.Likes.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task ToggleLikeAsync_ShouldRemoveLike_AndReturnFalse_WhenAlreadyLiked()
        {
            using var fx = new SqliteDatabaseFixture();
            var (ctx, user, post) = await SeedSqliteAsync(fx);
            ctx.Likes.Add(new Like(post.Id, user.Id) { User = user, Post = post });
            await ctx.SaveChangesAsync();

            var repo = new LikeRepository(ctx);
            var result = await repo.ToggleLikeAsync(user.Id, post.Id);

            result.Should().BeFalse();
            (await ctx.Likes.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task ToggleLikeAsync_ShouldOnlyRemove_LikeMatchingUserAndPost()
        {
            using var fx = new SqliteDatabaseFixture();
            var (ctx, user, post) = await SeedSqliteAsync(fx);
            var otherUser = new UserBuilder().Build();
            ctx.Users.Add(otherUser);
            await ctx.SaveChangesAsync();

            ctx.Likes.AddRange(
                new Like(post.Id, user.Id) { User = user, Post = post },
                new Like(post.Id, otherUser.Id) { User = otherUser, Post = post });
            await ctx.SaveChangesAsync();

            var repo = new LikeRepository(ctx);
            var result = await repo.ToggleLikeAsync(user.Id, post.Id);

            result.Should().BeFalse();
            (await ctx.Likes.CountAsync()).Should().Be(1);
            (await ctx.Likes.FirstAsync()).UserId.Should().Be(otherUser.Id);
        }

        [Fact]
        public async Task ToggleLikeAsync_ShouldAllowReLike_AfterUnlike()
        {
            using var fx = new SqliteDatabaseFixture();
            var (ctx, user, post) = await SeedSqliteAsync(fx);
            var repo = new LikeRepository(ctx);

            var liked = await repo.ToggleLikeAsync(user.Id, post.Id);
            var unliked = await repo.ToggleLikeAsync(user.Id, post.Id);
            var reliked = await repo.ToggleLikeAsync(user.Id, post.Id);

            liked.Should().BeTrue();
            unliked.Should().BeFalse();
            reliked.Should().BeTrue();
            (await ctx.Likes.CountAsync()).Should().Be(1);
        }
    }
}
