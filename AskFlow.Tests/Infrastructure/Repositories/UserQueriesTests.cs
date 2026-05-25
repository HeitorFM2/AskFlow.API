using AskFlow.Infrastructure.Repositories;
using AskFlow.Tests.Common.Builders;
using AskFlow.Tests.Common.Fixtures;

namespace AskFlow.Tests.Infrastructure.Repositories
{
    public class UserQueriesTests
    {
        [Fact]
        public async Task GetAllAsync_ShouldReturn_Empty_WhenNoUsersExist()
        {
            using var fx = new DatabaseFixture();
            var queries = new UserQueries(fx.Context);

            var result = await queries.GetAllAsync("excluded-id");

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_ShouldExclude_TheSpecifiedUser()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var excluded = new UserBuilder().WithId("excluded").Build();
            var other = new UserBuilder().Build();
            ctx.Users.AddRange(excluded, other);
            await ctx.SaveChangesAsync();
            var queries = new UserQueries(ctx);

            var result = await queries.GetAllAsync("excluded");

            result.Should().ContainSingle();
            result[0].UserName.Should().Be(other.UserName);
        }

        [Fact]
        public async Task GetAllAsync_ShouldProjectAllFields_Correctly()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var user = new UserBuilder()
                .WithEmail("alice@test.com")
                .WithIdentification("alice_id")
                .WithAvatarUrl("https://img/alice.png")
                .Build();
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();
            var queries = new UserQueries(ctx);

            var result = await queries.GetAllAsync("other-id");

            var dto = result.Should().ContainSingle().Subject;
            dto.UserName.Should().Be(user.UserName);
            dto.Identification.Should().Be("alice_id");
            dto.AvatarUrl.Should().Be("https://img/alice.png");
            dto.IsFollowing.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllAsync_WithoutSearch_ShouldReturnAllUsers_ExceptExcluded()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var excluded = new UserBuilder().WithId("me").Build();
            var u1 = new UserBuilder().Build();
            var u2 = new UserBuilder().Build();
            ctx.Users.AddRange(excluded, u1, u2);
            await ctx.SaveChangesAsync();
            var queries = new UserQueries(ctx);

            var result = await queries.GetAllAsync("me");

            result.Should().HaveCount(2);
            result.Select(u => u.UserName).Should().NotContain(excluded.UserName);
        }

        [Fact]
        public async Task GetAllAsync_ShouldFilter_BySearch_OnUserName()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var matching = new UserBuilder().WithEmail("alice@test.com").Build();
            var nonMatching = new UserBuilder().WithEmail("bob@test.com").Build();
            ctx.Users.AddRange(matching, nonMatching);
            await ctx.SaveChangesAsync();
            var queries = new UserQueries(ctx);

            var result = await queries.GetAllAsync("other-id", "alice");

            result.Should().ContainSingle();
            result[0].UserName.Should().Be(matching.UserName);
        }

        [Fact]
        public async Task GetAllAsync_ShouldFilter_BySearch_OnIdentification()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var matching = new UserBuilder().WithIdentification("alice_handle").Build();
            var nonMatching = new UserBuilder().WithIdentification("bob_handle").Build();
            ctx.Users.AddRange(matching, nonMatching);
            await ctx.SaveChangesAsync();
            var queries = new UserQueries(ctx);

            var result = await queries.GetAllAsync("other-id", "alice_handle");

            result.Should().ContainSingle();
            result[0].Identification.Should().Be("alice_handle");
        }

        [Fact]
        public async Task GetAllAsync_WhenExcludedUserMatchesSearch_ShouldStillExcludeThem()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var excluded = new UserBuilder().WithId("me").WithEmail("alice@test.com").Build();
            var other = new UserBuilder().WithEmail("alice2@test.com").Build();
            ctx.Users.AddRange(excluded, other);
            await ctx.SaveChangesAsync();
            var queries = new UserQueries(ctx);

            var result = await queries.GetAllAsync("me", "alice");

            result.Should().ContainSingle();
            result[0].UserName.Should().Be(other.UserName);
        }

        [Fact]
        public async Task GetAllAsync_WithSearch_ShouldReturn_Empty_WhenNobodyMatches()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var user = new UserBuilder().WithEmail("bob@test.com").WithIdentification("bob_id").Build();
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();
            var queries = new UserQueries(ctx);

            var result = await queries.GetAllAsync("other-id", "alice");

            result.Should().BeEmpty();
        }

        // --- IsIdentificationTakenAsync ---

        [Fact]
        public async Task IsIdentificationTakenAsync_ShouldReturnFalse_WhenNoOtherUserHasThatIdentification()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var user = new UserBuilder().WithIdentification("unique_id").Build();
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();
            var queries = new UserQueries(ctx);

            var result = await queries.IsIdentificationTakenAsync("unique_id", user.Id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsIdentificationTakenAsync_ShouldReturnTrue_WhenAnotherUserHasThatIdentification()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var existing = new UserBuilder().WithIdentification("taken_id").Build();
            var other = new UserBuilder().Build();
            ctx.Users.AddRange(existing, other);
            await ctx.SaveChangesAsync();
            var queries = new UserQueries(ctx);

            var result = await queries.IsIdentificationTakenAsync("taken_id", other.Id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsIdentificationTakenAsync_ShouldReturnFalse_WhenIdentificationDoesNotExist()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            var user = new UserBuilder().Build();
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();
            var queries = new UserQueries(ctx);

            var result = await queries.IsIdentificationTakenAsync("nonexistent_id", user.Id);

            result.Should().BeFalse();
        }
    }
}
