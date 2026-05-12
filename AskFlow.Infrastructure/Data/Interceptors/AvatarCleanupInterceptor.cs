using AskFlow.Application.Interfaces;
using AskFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace AskFlow.Infrastructure.Data.Interceptors
{
    public class AvatarCleanupInterceptor(
        IAvatarStorage avatarStorage,
        ILogger<AvatarCleanupInterceptor> logger) : SaveChangesInterceptor
    {
        private List<string> _deletedUserIds = [];

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            _deletedUserIds = eventData.Context?.ChangeTracker.Entries<User>()
                .Where(e => e.State == EntityState.Deleted)
                .Select(e => e.Entity.Id)
                .ToList() ?? [];

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            foreach (var userId in _deletedUserIds)
            {
                try
                {
                    await avatarStorage.DeleteAsync(userId, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to delete avatar for user {UserId} after deletion.", userId);
                }
            }

            _deletedUserIds = [];
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }
    }
}
