using AskFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AskFlow.Tests.Common.Fixtures
{
    public sealed class DatabaseFixture : IDisposable
    {
        public AppDbContext Context { get; }

        public DatabaseFixture()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"AskFlow-Tests-{Guid.NewGuid()}")
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            Context = new AppDbContext(options);
            Context.Database.EnsureCreated();
        }

        public AppDbContext NewContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Context.Database.GetDbConnection().Database)
                .Options;
            return new AppDbContext(options);
        }

        public void Dispose()
        {
            try
            {
                Context.Database.EnsureDeleted();
            }
            catch (ObjectDisposedException)
            {
            }
            Context.Dispose();
        }
    }
}
