using AskFlow.Domain.Entities;
using AskFlow.Infrastructure.Data;
using AskFlow.Infrastructure.Services;
using AskFlow.Tests.Common.Builders;
using AskFlow.Tests.Common.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AskFlow.Tests.Infrastructure.Services
{
    public class RefreshTokenCleanupServiceTests
    {
        private static IServiceProvider CreateProvider(AppDbContext context)
        {
            var services = new ServiceCollection();
            services.AddSingleton(context);
            return services.BuildServiceProvider();
        }

        private static async Task<User> SeedUserAsync(AppDbContext context)
        {
            var user = new UserBuilder().Build();
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }

        [Fact]
        public async Task CleanupAsync_ShouldDelete_TokensExpiredBeyondRetention()
        {
            using var fx = new SqliteDatabaseFixture();
            var user = await SeedUserAsync(fx.Context);

            fx.Context.RefreshTokens.AddRange(
                new RefreshToken("very-old", user, DateTime.UtcNow.AddDays(-40)),
                new RefreshToken("ancient", user, DateTime.UtcNow.AddDays(-365)));
            await fx.Context.SaveChangesAsync();

            var sut = new RefreshTokenCleanupService(CreateProvider(fx.Context), Substitute.For<ILogger<RefreshTokenCleanupService>>());

            await sut.CleanupAsync(default);

            (await fx.Context.RefreshTokens.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task CleanupAsync_ShouldKeep_TokensExpiredWithinRetention()
        {
            using var fx = new SqliteDatabaseFixture();
            var user = await SeedUserAsync(fx.Context);

            fx.Context.RefreshTokens.Add(new RefreshToken("recent", user, DateTime.UtcNow.AddDays(-10)));
            await fx.Context.SaveChangesAsync();

            var sut = new RefreshTokenCleanupService(CreateProvider(fx.Context), Substitute.For<ILogger<RefreshTokenCleanupService>>());

            await sut.CleanupAsync(default);

            (await fx.Context.RefreshTokens.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task CleanupAsync_ShouldKeep_ActiveTokens()
        {
            using var fx = new SqliteDatabaseFixture();
            var user = await SeedUserAsync(fx.Context);

            fx.Context.RefreshTokens.Add(new RefreshToken("future", user, DateTime.UtcNow.AddDays(10)));
            await fx.Context.SaveChangesAsync();

            var sut = new RefreshTokenCleanupService(CreateProvider(fx.Context), Substitute.For<ILogger<RefreshTokenCleanupService>>());

            await sut.CleanupAsync(default);

            (await fx.Context.RefreshTokens.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task CleanupAsync_MixedTokens_ShouldDeleteOnlyExpiredBeyondRetention()
        {
            using var fx = new SqliteDatabaseFixture();
            var user = await SeedUserAsync(fx.Context);

            fx.Context.RefreshTokens.AddRange(
                new RefreshToken("very-old", user, DateTime.UtcNow.AddDays(-40)),
                new RefreshToken("recent", user, DateTime.UtcNow.AddDays(-10)),
                new RefreshToken("future", user, DateTime.UtcNow.AddDays(10)));
            await fx.Context.SaveChangesAsync();

            var sut = new RefreshTokenCleanupService(CreateProvider(fx.Context), Substitute.For<ILogger<RefreshTokenCleanupService>>());

            await sut.CleanupAsync(default);

            var remaining = await fx.Context.RefreshTokens
                .AsNoTracking()
                .Select(t => t.TokenHash)
                .ToListAsync();

            remaining.Should().HaveCount(2);
            remaining.Should().NotContain(RefreshToken.HashToken("very-old"));
            remaining.Should().Contain(RefreshToken.HashToken("recent"));
            remaining.Should().Contain(RefreshToken.HashToken("future"));
        }

        [Fact]
        public async Task CleanupAsync_NoTokens_ShouldNotThrow()
        {
            using var fx = new SqliteDatabaseFixture();
            var sut = new RefreshTokenCleanupService(CreateProvider(fx.Context), Substitute.For<ILogger<RefreshTokenCleanupService>>());

            var act = async () => await sut.CleanupAsync(default);

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task CleanupAsync_ShouldLog_WhenTokensDeleted()
        {
            using var fx = new SqliteDatabaseFixture();
            var user = await SeedUserAsync(fx.Context);
            fx.Context.RefreshTokens.Add(new RefreshToken("very-old", user, DateTime.UtcNow.AddDays(-40)));
            await fx.Context.SaveChangesAsync();

            var logger = Substitute.For<ILogger<RefreshTokenCleanupService>>();
            var sut = new RefreshTokenCleanupService(CreateProvider(fx.Context), logger);

            await sut.CleanupAsync(default);

            logger.ReceivedCalls()
                .Any(c => c.GetMethodInfo().Name == "Log" && c.GetArguments()[0]?.ToString() == "Information")
                .Should().BeTrue();
        }

        [Fact]
        public async Task CleanupAsync_ShouldNotLog_WhenNoTokensDeleted()
        {
            using var fx = new SqliteDatabaseFixture();
            var logger = Substitute.For<ILogger<RefreshTokenCleanupService>>();
            var sut = new RefreshTokenCleanupService(CreateProvider(fx.Context), logger);

            await sut.CleanupAsync(default);

            logger.ReceivedCalls()
                .Any(c => c.GetMethodInfo().Name == "Log" && c.GetArguments()[0]?.ToString() == "Information")
                .Should().BeFalse();
        }

        [Fact]
        public async Task CleanupAsync_ShouldNotAffect_OtherEntities()
        {
            using var fx = new SqliteDatabaseFixture();
            var user = await SeedUserAsync(fx.Context);
            var post = new PostBuilder().WithId(0).WithUserId(user.Id).Build();
            fx.Context.Posts.Add(post);
            fx.Context.RefreshTokens.Add(new RefreshToken("very-old", user, DateTime.UtcNow.AddDays(-40)));
            await fx.Context.SaveChangesAsync();

            var sut = new RefreshTokenCleanupService(CreateProvider(fx.Context), Substitute.For<ILogger<RefreshTokenCleanupService>>());

            await sut.CleanupAsync(default);

            (await fx.Context.Users.CountAsync()).Should().Be(1);
            (await fx.Context.Posts.CountAsync()).Should().Be(1);
            (await fx.Context.RefreshTokens.CountAsync()).Should().Be(0);
        }
    }
}
