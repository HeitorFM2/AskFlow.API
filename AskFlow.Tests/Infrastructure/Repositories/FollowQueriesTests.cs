using AskFlow.Domain.Entities;
using AskFlow.Infrastructure.Repositories;
using AskFlow.Tests.Common.Builders;
using AskFlow.Tests.Common.Fixtures;

namespace AskFlow.Tests.Infrastructure.Repositories
{
    public class FollowQueriesTests
    {
        private static async Task<(AskFlow.Infrastructure.Data.AppDbContext ctx, User follower, User followed)> SeedWithFollowAsync(DatabaseFixture fx)
        {
            var ctx = fx.Context;
            var follower = new UserBuilder().Build();
            var followed = new UserBuilder().Build();
            ctx.Users.AddRange(follower, followed);
            await ctx.SaveChangesAsync();
            ctx.Follows.Add(Follow.Create(follower.Id, followed.Id));
            await ctx.SaveChangesAsync();
            return (ctx, follower, followed);
        }

        private static async Task<(AskFlow.Infrastructure.Data.AppDbContext ctx, User u1, User u2)> SeedUsersAsync(DatabaseFixture fx)
        {
            var ctx = fx.Context;
            var u1 = new UserBuilder().Build();
            var u2 = new UserBuilder().Build();
            ctx.Users.AddRange(u1, u2);
            await ctx.SaveChangesAsync();
            return (ctx, u1, u2);
        }

        // --- GetFollowersAsync ---

        [Fact]
        public async Task GetFollowersAsync_ShouldReturn_Empty_WhenUserHasNoFollowers()
        {
            using var fx = new DatabaseFixture();
            var (ctx, _, followed) = await SeedUsersAsync(fx);
            var queries = new FollowQueries(ctx);

            var result = await queries.GetFollowersAsync(followed.Id, 1, 20);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetFollowersAsync_ShouldReturn_OnlyFollowersOfSpecifiedUser()
        {
            using var fx = new DatabaseFixture();
            var (ctx, follower, followed) = await SeedWithFollowAsync(fx);
            var unrelated = new UserBuilder().Build();
            ctx.Users.Add(unrelated);
            await ctx.SaveChangesAsync();
            var queries = new FollowQueries(ctx);

            var result = await queries.GetFollowersAsync(followed.Id, 1, 20);

            result.Should().ContainSingle();
            result[0].UserName.Should().Be(follower.UserName);
        }

        [Fact]
        public async Task GetFollowersAsync_ShouldProjectAllFields()
        {
            using var fx = new DatabaseFixture();
            var (ctx, follower, followed) = await SeedWithFollowAsync(fx);
            var queries = new FollowQueries(ctx);

            var result = await queries.GetFollowersAsync(followed.Id, 1, 20);

            var vm = result.Should().ContainSingle().Subject;
            vm.UserName.Should().Be(follower.UserName);
            vm.Identification.Should().Be(follower.Identification);
            vm.AvatarUrl.Should().Be(follower.AvatarUrl);
        }

        [Fact]
        public async Task GetFollowersAsync_ShouldFilter_BySearch_OnUserName()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var followed = new UserBuilder().Build();
            var matchingFollower = new UserBuilder().WithEmail("alice@test.com").Build();
            var nonMatchingFollower = new UserBuilder().WithEmail("bob@test.com").Build();
            ctx.Users.AddRange(followed, matchingFollower, nonMatchingFollower);
            await ctx.SaveChangesAsync();
            ctx.Follows.Add(Follow.Create(matchingFollower.Id, followed.Id));
            ctx.Follows.Add(Follow.Create(nonMatchingFollower.Id, followed.Id));
            await ctx.SaveChangesAsync();
            var queries = new FollowQueries(ctx);

            var result = await queries.GetFollowersAsync(followed.Id, 1, 20, "alice");

            result.Should().ContainSingle();
            result[0].UserName.Should().Be(matchingFollower.UserName);
        }

        [Fact]
        public async Task GetFollowersAsync_ShouldFilter_BySearch_OnIdentification()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var followed = new UserBuilder().Build();
            var matchingFollower = new UserBuilder().WithIdentification("alice_id").Build();
            var nonMatchingFollower = new UserBuilder().WithIdentification("bob_id").Build();
            ctx.Users.AddRange(followed, matchingFollower, nonMatchingFollower);
            await ctx.SaveChangesAsync();
            ctx.Follows.Add(Follow.Create(matchingFollower.Id, followed.Id));
            ctx.Follows.Add(Follow.Create(nonMatchingFollower.Id, followed.Id));
            await ctx.SaveChangesAsync();
            var queries = new FollowQueries(ctx);

            var result = await queries.GetFollowersAsync(followed.Id, 1, 20, "alice_id");

            result.Should().ContainSingle();
            result[0].Identification.Should().Be("alice_id");
        }

        [Fact]
        public async Task GetFollowersAsync_ShouldOrder_ByCreatedAt_Descending()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var followed = new UserBuilder().Build();
            var first = new UserBuilder().Build();
            var second = new UserBuilder().Build();
            ctx.Users.AddRange(followed, first, second);
            await ctx.SaveChangesAsync();

            ctx.Follows.Add(Follow.Create(first.Id, followed.Id));
            await ctx.SaveChangesAsync();
            await Task.Delay(10);
            ctx.Follows.Add(Follow.Create(second.Id, followed.Id));
            await ctx.SaveChangesAsync();

            var queries = new FollowQueries(ctx);
            var result = await queries.GetFollowersAsync(followed.Id, 1, 20);

            result.Select(f => f.UserName).Should().Equal(second.UserName, first.UserName);
        }

        [Fact]
        public async Task GetFollowersAsync_ShouldRespect_Pagination()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var followed = new UserBuilder().Build();
            ctx.Users.Add(followed);
            await ctx.SaveChangesAsync();

            for (int i = 0; i < 5; i++)
            {
                var f = new UserBuilder().Build();
                ctx.Users.Add(f);
                await ctx.SaveChangesAsync();
                ctx.Follows.Add(Follow.Create(f.Id, followed.Id));
                await ctx.SaveChangesAsync();
                await Task.Delay(5);
            }

            var queries = new FollowQueries(ctx);
            var page1 = await queries.GetFollowersAsync(followed.Id, 1, 3);
            var page2 = await queries.GetFollowersAsync(followed.Id, 2, 3);

            page1.Should().HaveCount(3);
            page2.Should().HaveCount(2);
            page1.Select(f => f.UserName).Should().NotIntersectWith(page2.Select(f => f.UserName));
        }

        // --- CountFollowersAsync ---

        [Fact]
        public async Task CountFollowersAsync_ShouldReturn_Zero_WhenNoFollowers()
        {
            using var fx = new DatabaseFixture();
            var (ctx, _, followed) = await SeedUsersAsync(fx);
            var queries = new FollowQueries(ctx);

            (await queries.CountFollowersAsync(followed.Id)).Should().Be(0);
        }

        [Fact]
        public async Task CountFollowersAsync_ShouldReturn_CorrectCount()
        {
            using var fx = new DatabaseFixture();
            var (ctx, _, followed) = await SeedWithFollowAsync(fx);
            var follower2 = new UserBuilder().Build();
            ctx.Users.Add(follower2);
            await ctx.SaveChangesAsync();
            ctx.Follows.Add(Follow.Create(follower2.Id, followed.Id));
            await ctx.SaveChangesAsync();
            var queries = new FollowQueries(ctx);

            (await queries.CountFollowersAsync(followed.Id)).Should().Be(2);
        }

        [Fact]
        public async Task CountFollowersAsync_ShouldFilter_BySearch()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var followed = new UserBuilder().Build();
            var matchingFollower = new UserBuilder().WithIdentification("alice_ident").Build();
            var nonMatchingFollower = new UserBuilder().Build();
            ctx.Users.AddRange(followed, matchingFollower, nonMatchingFollower);
            await ctx.SaveChangesAsync();
            ctx.Follows.Add(Follow.Create(matchingFollower.Id, followed.Id));
            ctx.Follows.Add(Follow.Create(nonMatchingFollower.Id, followed.Id));
            await ctx.SaveChangesAsync();
            var queries = new FollowQueries(ctx);

            (await queries.CountFollowersAsync(followed.Id, "alice_ident")).Should().Be(1);
        }

        [Fact]
        public async Task CountFollowersAsync_ShouldNotCount_FollowersOfOtherUsers()
        {
            using var fx = new DatabaseFixture();
            var (ctx, follower, followed) = await SeedWithFollowAsync(fx);
            var otherUser = new UserBuilder().Build();
            ctx.Users.Add(otherUser);
            await ctx.SaveChangesAsync();
            ctx.Follows.Add(Follow.Create(follower.Id, otherUser.Id));
            await ctx.SaveChangesAsync();
            var queries = new FollowQueries(ctx);

            (await queries.CountFollowersAsync(followed.Id)).Should().Be(1);
        }

        // --- GetFollowingAsync ---

        [Fact]
        public async Task GetFollowingAsync_ShouldReturn_Empty_WhenUserFollowsNobody()
        {
            using var fx = new DatabaseFixture();
            var (ctx, follower, _) = await SeedUsersAsync(fx);
            var queries = new FollowQueries(ctx);

            var result = await queries.GetFollowingAsync(follower.Id, 1, 20);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetFollowingAsync_ShouldReturn_OnlyUsersFollowedBySpecifiedUser()
        {
            using var fx = new DatabaseFixture();
            var (ctx, follower, followed) = await SeedWithFollowAsync(fx);
            var queries = new FollowQueries(ctx);

            var result = await queries.GetFollowingAsync(follower.Id, 1, 20);

            result.Should().ContainSingle();
            result[0].UserName.Should().Be(followed.UserName);
        }

        [Fact]
        public async Task GetFollowingAsync_ShouldAlwaysSetIsFollowing_True()
        {
            using var fx = new DatabaseFixture();
            var (ctx, follower, _) = await SeedWithFollowAsync(fx);
            var followed2 = new UserBuilder().Build();
            ctx.Users.Add(followed2);
            await ctx.SaveChangesAsync();
            ctx.Follows.Add(Follow.Create(follower.Id, followed2.Id));
            await ctx.SaveChangesAsync();
            var queries = new FollowQueries(ctx);

            var result = await queries.GetFollowingAsync(follower.Id, 1, 20);

            result.Should().AllSatisfy(f => f.IsFollowing.Should().BeTrue());
        }

        [Fact]
        public async Task GetFollowingAsync_ShouldFilter_BySearch_OnUserName()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var follower = new UserBuilder().Build();
            var matchedFollowed = new UserBuilder().WithEmail("carol@test.com").Build();
            var unmatchedFollowed = new UserBuilder().WithEmail("dave@test.com").Build();
            ctx.Users.AddRange(follower, matchedFollowed, unmatchedFollowed);
            await ctx.SaveChangesAsync();
            ctx.Follows.Add(Follow.Create(follower.Id, matchedFollowed.Id));
            ctx.Follows.Add(Follow.Create(follower.Id, unmatchedFollowed.Id));
            await ctx.SaveChangesAsync();
            var queries = new FollowQueries(ctx);

            var result = await queries.GetFollowingAsync(follower.Id, 1, 20, "carol");

            result.Should().ContainSingle();
            result[0].UserName.Should().Be(matchedFollowed.UserName);
        }

        [Fact]
        public async Task GetFollowingAsync_ShouldFilter_BySearch_OnIdentification()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var follower = new UserBuilder().Build();
            var matchedFollowed = new UserBuilder().WithIdentification("carol_id").Build();
            var unmatchedFollowed = new UserBuilder().Build();
            ctx.Users.AddRange(follower, matchedFollowed, unmatchedFollowed);
            await ctx.SaveChangesAsync();
            ctx.Follows.Add(Follow.Create(follower.Id, matchedFollowed.Id));
            ctx.Follows.Add(Follow.Create(follower.Id, unmatchedFollowed.Id));
            await ctx.SaveChangesAsync();
            var queries = new FollowQueries(ctx);

            var result = await queries.GetFollowingAsync(follower.Id, 1, 20, "carol_id");

            result.Should().ContainSingle();
            result[0].Identification.Should().Be("carol_id");
        }

        [Fact]
        public async Task GetFollowingAsync_ShouldRespect_Pagination()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var follower = new UserBuilder().Build();
            ctx.Users.Add(follower);
            await ctx.SaveChangesAsync();

            for (int i = 0; i < 5; i++)
            {
                var f = new UserBuilder().Build();
                ctx.Users.Add(f);
                await ctx.SaveChangesAsync();
                ctx.Follows.Add(Follow.Create(follower.Id, f.Id));
                await ctx.SaveChangesAsync();
                await Task.Delay(5);
            }

            var queries = new FollowQueries(ctx);
            var page1 = await queries.GetFollowingAsync(follower.Id, 1, 3);
            var page2 = await queries.GetFollowingAsync(follower.Id, 2, 3);

            page1.Should().HaveCount(3);
            page2.Should().HaveCount(2);
            page1.Select(f => f.UserName).Should().NotIntersectWith(page2.Select(f => f.UserName));
        }

        // --- CountFollowingAsync ---

        [Fact]
        public async Task CountFollowingAsync_ShouldReturn_Zero_WhenFollowingNobody()
        {
            using var fx = new DatabaseFixture();
            var (ctx, follower, _) = await SeedUsersAsync(fx);
            var queries = new FollowQueries(ctx);

            (await queries.CountFollowingAsync(follower.Id)).Should().Be(0);
        }

        [Fact]
        public async Task CountFollowingAsync_ShouldReturn_CorrectCount()
        {
            using var fx = new DatabaseFixture();
            var (ctx, follower, _) = await SeedWithFollowAsync(fx);
            var followed2 = new UserBuilder().Build();
            ctx.Users.Add(followed2);
            await ctx.SaveChangesAsync();
            ctx.Follows.Add(Follow.Create(follower.Id, followed2.Id));
            await ctx.SaveChangesAsync();
            var queries = new FollowQueries(ctx);

            (await queries.CountFollowingAsync(follower.Id)).Should().Be(2);
        }

        [Fact]
        public async Task CountFollowingAsync_ShouldFilter_BySearch()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var follower = new UserBuilder().Build();
            var matchedFollowed = new UserBuilder().WithIdentification("matched_id").Build();
            var unmatchedFollowed = new UserBuilder().Build();
            ctx.Users.AddRange(follower, matchedFollowed, unmatchedFollowed);
            await ctx.SaveChangesAsync();
            ctx.Follows.Add(Follow.Create(follower.Id, matchedFollowed.Id));
            ctx.Follows.Add(Follow.Create(follower.Id, unmatchedFollowed.Id));
            await ctx.SaveChangesAsync();
            var queries = new FollowQueries(ctx);

            (await queries.CountFollowingAsync(follower.Id, "matched_id")).Should().Be(1);
        }

        [Fact]
        public async Task CountFollowingAsync_ShouldNotCount_FollowsFromOtherUsers()
        {
            using var fx = new DatabaseFixture();
            var (ctx, _, followed) = await SeedWithFollowAsync(fx);
            var otherFollower = new UserBuilder().Build();
            ctx.Users.Add(otherFollower);
            await ctx.SaveChangesAsync();
            ctx.Follows.Add(Follow.Create(otherFollower.Id, followed.Id));
            await ctx.SaveChangesAsync();
            var queries = new FollowQueries(ctx);

            (await queries.CountFollowingAsync(otherFollower.Id)).Should().Be(1);
            (await queries.CountFollowingAsync(followed.Id)).Should().Be(0);
        }

        // --- GetFollowedUserNamesAsync ---

        [Fact]
        public async Task GetFollowedUserNamesAsync_ShouldReturn_EmptyHashSet_WhenUserNamesIsEmpty()
        {
            using var fx = new DatabaseFixture();
            var (ctx, follower, _) = await SeedUsersAsync(fx);
            var queries = new FollowQueries(ctx);

            var result = await queries.GetFollowedUserNamesAsync(follower.Id, []);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetFollowedUserNamesAsync_ShouldReturn_OnlyFollowedUsers_FromGivenList()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var follower = new UserBuilder().Build();
            var followedA = new UserBuilder().Build();
            var followedB = new UserBuilder().Build();
            var notFollowed = new UserBuilder().Build();
            ctx.Users.AddRange(follower, followedA, followedB, notFollowed);
            await ctx.SaveChangesAsync();
            ctx.Follows.Add(Follow.Create(follower.Id, followedA.Id));
            ctx.Follows.Add(Follow.Create(follower.Id, followedB.Id));
            await ctx.SaveChangesAsync();
            var queries = new FollowQueries(ctx);

            var result = await queries.GetFollowedUserNamesAsync(
                follower.Id, [followedA.UserName!, followedB.UserName!, notFollowed.UserName!]);

            result.Should().BeEquivalentTo(new[] { followedA.UserName, followedB.UserName });
        }

        [Fact]
        public async Task GetFollowedUserNamesAsync_ShouldReturn_Empty_WhenNoneAreFollowed()
        {
            using var fx = new DatabaseFixture();
            var (ctx, follower, notFollowed) = await SeedUsersAsync(fx);
            var queries = new FollowQueries(ctx);

            var result = await queries.GetFollowedUserNamesAsync(follower.Id, [notFollowed.UserName!]);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetFollowedUserNamesAsync_ShouldNotReturn_UsersFollowedByOtherUsers()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var follower = new UserBuilder().Build();
            var followed = new UserBuilder().Build();
            var otherFollower = new UserBuilder().Build();
            ctx.Users.AddRange(follower, followed, otherFollower);
            await ctx.SaveChangesAsync();
            ctx.Follows.Add(Follow.Create(otherFollower.Id, followed.Id));
            await ctx.SaveChangesAsync();
            var queries = new FollowQueries(ctx);

            var result = await queries.GetFollowedUserNamesAsync(follower.Id, [followed.UserName!]);

            result.Should().BeEmpty();
        }

        // --- IsFollowingAsync ---

        [Fact]
        public async Task IsFollowingAsync_ShouldReturnTrue_WhenFollowExists()
        {
            using var fx = new DatabaseFixture();
            var (ctx, follower, followed) = await SeedWithFollowAsync(fx);
            var queries = new FollowQueries(ctx);

            var result = await queries.IsFollowingAsync(follower.Id, followed.UserName!);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsFollowingAsync_ShouldReturnFalse_WhenFollowDoesNotExist()
        {
            using var fx = new DatabaseFixture();
            var (ctx, follower, followed) = await SeedUsersAsync(fx);
            var queries = new FollowQueries(ctx);

            var result = await queries.IsFollowingAsync(follower.Id, followed.UserName!);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsFollowingAsync_ShouldReturnFalse_WhenFollowBelongsToAnotherUser()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var follower = new UserBuilder().Build();
            var followed = new UserBuilder().Build();
            var otherUser = new UserBuilder().Build();
            ctx.Users.AddRange(follower, followed, otherUser);
            await ctx.SaveChangesAsync();
            ctx.Follows.Add(Follow.Create(otherUser.Id, followed.Id));
            await ctx.SaveChangesAsync();
            var queries = new FollowQueries(ctx);

            var result = await queries.IsFollowingAsync(follower.Id, followed.UserName!);

            result.Should().BeFalse();
        }
    }
}
