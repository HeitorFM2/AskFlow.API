using AskFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AskFlow.Infrastructure.Services
{
    public class RefreshTokenCleanupService(
        IServiceProvider services,
        ILogger<RefreshTokenCleanupService> logger) : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromHours(6);
        private static readonly TimeSpan Retention = TimeSpan.FromDays(30);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(Interval);

            do
            {
                try
                {
                    await CleanupAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to clean up refresh tokens.");
                }
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        internal async Task CleanupAsync(CancellationToken cancellationToken)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var cutoff = DateTime.UtcNow - Retention;

            var deleted = await context.RefreshTokens
                .Where(t => t.ExpiresAt < cutoff)
                .ExecuteDeleteAsync(cancellationToken);

            if (deleted > 0)
                logger.LogInformation("Deleted {Count} expired refresh tokens.", deleted);
        }
    }
}
