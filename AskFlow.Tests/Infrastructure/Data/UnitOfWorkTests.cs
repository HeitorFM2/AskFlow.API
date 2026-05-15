using AskFlow.Infrastructure.Data;
using AskFlow.Tests.Common.Builders;
using AskFlow.Tests.Common.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace AskFlow.Tests.Infrastructure.Data
{
    public class UnitOfWorkTests
    {
        [Fact]
        public async Task SaveChangesAsync_ShouldPersistPendingChanges_AndReturnAffectedCount()
        {
            using var fx = new DatabaseFixture();
            var ctx = fx.Context;
            ctx.Users.Add(new UserBuilder().Build());

            var sut = new UnitOfWork(ctx);
            var affected = await sut.SaveChangesAsync();

            affected.Should().Be(1);
            (await ctx.Users.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task SaveChangesAsync_ShouldReturnZero_WhenNoPendingChanges()
        {
            using var fx = new DatabaseFixture();

            var sut = new UnitOfWork(fx.Context);

            (await sut.SaveChangesAsync()).Should().Be(0);
        }
    }
}
