using AskFlow.Application.Common;
using AskFlow.Application.Follows.Handlers;
using AskFlow.Application.Follows.Queries;
using AskFlow.Application.Interfaces;
using AskFlow.Tests.Common.Fixtures;

namespace AskFlow.Tests.Application.Follows.Handlers
{
    public class GetIsFollowingHandlerTests
    {
        private readonly IFollowQueries _queries = Substitute.For<IFollowQueries>();

        private GetIsFollowingHandler CreateSut(string? userId) =>
            new(_queries,
                userId is null
                    ? HttpContextFixture.CreateUnauthenticated()
                    : HttpContextFixture.CreateAuthenticated(userId));

        [Fact]
        public async Task Handle_Unauthenticated_ShouldReturnUnauthorized()
        {
            var result = await CreateSut(null).Handle(new GetIsFollowingQuery("alice"), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotAuthenticated);
            await _queries.DidNotReceive().IsFollowingAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Authenticated_WhenFollowing_ShouldReturnTrue()
        {
            _queries.IsFollowingAsync("u1", "alice", Arg.Any<CancellationToken>()).Returns(true);

            var result = await CreateSut("u1").Handle(new GetIsFollowingQuery("alice"), default);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_Authenticated_WhenNotFollowing_ShouldReturnFalse()
        {
            _queries.IsFollowingAsync("u1", "alice", Arg.Any<CancellationToken>()).Returns(false);

            var result = await CreateSut("u1").Handle(new GetIsFollowingQuery("alice"), default);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldPassTargetUserName_ToQuery()
        {
            _queries.IsFollowingAsync("u1", "bob", Arg.Any<CancellationToken>()).Returns(false);

            await CreateSut("u1").Handle(new GetIsFollowingQuery("bob"), default);

            await _queries.Received(1).IsFollowingAsync("u1", "bob", Arg.Any<CancellationToken>());
        }
    }
}
