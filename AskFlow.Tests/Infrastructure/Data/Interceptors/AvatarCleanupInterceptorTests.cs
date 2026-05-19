using AskFlow.Application.Interfaces;
using AskFlow.Infrastructure.Data;
using AskFlow.Infrastructure.Data.Interceptors;
using AskFlow.Tests.Common.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace AskFlow.Tests.Infrastructure.Data.Interceptors
{
    public class AvatarCleanupInterceptorTests
    {
        private readonly IAvatarStorage _avatarStorage = Substitute.For<IAvatarStorage>();
        private readonly ILogger<AvatarCleanupInterceptor> _logger = Substitute.For<ILogger<AvatarCleanupInterceptor>>();

        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"avatar-cleanup-{Guid.NewGuid()}")
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .AddInterceptors(new AvatarCleanupInterceptor(_avatarStorage, _logger))
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task SaveChanges_WhenUserIsDeleted_ShouldCallDeleteAsync_OnAvatarStorage()
        {
            await using var context = CreateContext();
            var user = new UserBuilder().WithId("u1").Build();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            context.Users.Remove(user);
            await context.SaveChangesAsync();

            await _avatarStorage.Received(1).DeleteAsync("u1", Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SaveChanges_WhenMultipleUsersDeleted_ShouldCallDeleteAsync_ForEach()
        {
            await using var context = CreateContext();
            var a = new UserBuilder().WithId("a").Build();
            var b = new UserBuilder().WithId("b").Build();
            context.Users.AddRange(a, b);
            await context.SaveChangesAsync();

            context.Users.RemoveRange(a, b);
            await context.SaveChangesAsync();

            await _avatarStorage.Received(1).DeleteAsync("a", Arg.Any<CancellationToken>());
            await _avatarStorage.Received(1).DeleteAsync("b", Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SaveChanges_WithNoUserDeletions_ShouldNotCallDeleteAsync()
        {
            await using var context = CreateContext();
            var user = new UserBuilder().Build();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            await _avatarStorage.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SaveChanges_WhenAvatarStorageThrows_ShouldNotPropagate_AndLogError()
        {
            _avatarStorage
                .When(s => s.DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()))
                .Do(_ => throw new InvalidOperationException("azure down"));

            await using var context = CreateContext();
            var user = new UserBuilder().WithId("u1").Build();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            context.Users.Remove(user);
            var act = async () => await context.SaveChangesAsync();

            await act.Should().NotThrowAsync();
            _logger.ReceivedCalls()
                .Any(c => c.GetMethodInfo().Name == "Log" && c.GetArguments()[0]?.ToString() == "Error")
                .Should().BeTrue();
        }

        [Fact]
        public async Task SavingChangesAsync_WhenContextIsNull_ShouldNotDeleteAvatar()
        {
            var interceptor = new AvatarCleanupInterceptor(_avatarStorage, _logger);
            var eventData = new DbContextEventData(null!, null!, (DbContext?)null);
            var completedData = new SaveChangesCompletedEventData(null!, null!, null!, 0);

            await interceptor.SavingChangesAsync(eventData, default);
            await interceptor.SavedChangesAsync(completedData, 0, default);

            await _avatarStorage.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SavingChangesAsync_WhenUserIsModified_ShouldNotDeleteAvatar()
        {
            await using var context = CreateContext();
            var user = new UserBuilder().WithId("u1").Build();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            user.Identification = "updated_identification";
            context.Users.Update(user);
            await context.SaveChangesAsync();

            await _avatarStorage.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }
    }
}
