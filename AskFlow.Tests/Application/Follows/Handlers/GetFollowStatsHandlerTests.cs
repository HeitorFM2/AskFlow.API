using AskFlow.Application.Common;
using AskFlow.Application.Follows.Handlers;
using AskFlow.Application.Follows.Queries;
using AskFlow.Application.Interfaces;
using AskFlow.Tests.Common.Fixtures;

namespace AskFlow.Tests.Application.Follows.Handlers
{
    public class GetFollowStatsHandlerTests
    {
        private readonly IFollowQueries _queries = Substitute.For<IFollowQueries>();

        [Fact]
        public async Task Handle_Unauthenticated_ShouldReturnUnauthorized()
        {
            var sut = new GetFollowStatsHandler(_queries, HttpContextFixture.CreateUnauthenticated());

            var result = await sut.Handle(new GetFollowStatsQuery(), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotAuthenticated);
            await _queries.DidNotReceive().CountFollowersAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
            await _queries.DidNotReceive().CountFollowingAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldReturnFollowersAndFollowingCounts()
        {
            const string userId = "u1";
            _queries.CountFollowersAsync(userId, cancellationToken: Arg.Any<CancellationToken>()).Returns(12);
            _queries.CountFollowingAsync(userId, cancellationToken: Arg.Any<CancellationToken>()).Returns(5);
            var sut = new GetFollowStatsHandler(_queries, HttpContextFixture.CreateAuthenticated(userId));

            var result = await sut.Handle(new GetFollowStatsQuery(), default);

            result.IsSuccess.Should().BeTrue();
            result.Value!.Followers.Should().Be(12);
            result.Value.Following.Should().Be(5);
        }
    }
}
