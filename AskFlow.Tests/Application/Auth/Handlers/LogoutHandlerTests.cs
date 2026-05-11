using AskFlow.Application.Auth.Commands;
using AskFlow.Application.Auth.Handlers;
using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;

namespace AskFlow.Tests.Application.Auth.Handlers
{
    public class LogoutHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldRevokeAllTokens_ForUser()
        {
            var repo = Substitute.For<IRefreshTokenRepository>();
            var sut = new LogoutHandler(repo);

            var result = await sut.Handle(new LogoutCommand("user-1"), default);

            result.Type.Should().Be(ResultType.Ok);
            await repo.Received(1).RevokeAllByUserIdAsync("user-1");
        }
    }
}
