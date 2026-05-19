using AskFlow.Application.Common;
using AskFlow.Application.Follows.Handlers;
using AskFlow.Application.Follows.Queries;
using AskFlow.Application.Follows.ViewModels;
using AskFlow.Application.Interfaces;
using AskFlow.Tests.Common.Fixtures;

namespace AskFlow.Tests.Application.Follows.Handlers
{
    public class GetFollowersHandlerTests
    {
        private readonly IFollowQueries _queries = Substitute.For<IFollowQueries>();

        [Fact]
        public async Task Handle_Unauthenticated_ShouldReturnUnauthorized()
        {
            var sut = new GetFollowersHandler(_queries, HttpContextFixture.CreateUnauthenticated());

            var result = await sut.Handle(new GetFollowersQuery(), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotAuthenticated);
            await _queries.DidNotReceive().GetFollowersAsync(
                Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Authenticated_EmptyFollowers_ShouldReturnEmptyPagedResult_AndSkipFollowedNamesQuery()
        {
            const string userId = "u1";
            _queries.GetFollowersAsync(userId, 1, 20, null, Arg.Any<CancellationToken>())
                .Returns(new List<FollowViewModel>());
            _queries.CountFollowersAsync(userId, null, Arg.Any<CancellationToken>()).Returns(0);
            var sut = new GetFollowersHandler(_queries, HttpContextFixture.CreateAuthenticated(userId));

            var result = await sut.Handle(new GetFollowersQuery(), default);

            result.IsSuccess.Should().BeTrue();
            result.Value!.Items.Should().BeEmpty();
            result.Value.TotalCount.Should().Be(0);
            await _queries.DidNotReceive().GetFollowedUserNamesAsync(
                Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldReturnPagedResult_WithMappedFields()
        {
            const string userId = "u1";
            var item = new FollowViewModel { UserName = "alice", Identification = "alice_id", AvatarUrl = "http://img" };
            _queries.GetFollowersAsync(userId, 1, 20, null, Arg.Any<CancellationToken>())
                .Returns(new List<FollowViewModel> { item });
            _queries.CountFollowersAsync(userId, null, Arg.Any<CancellationToken>()).Returns(1);
            _queries.GetFollowedUserNamesAsync(userId, Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
                .Returns(new HashSet<string>());
            var sut = new GetFollowersHandler(_queries, HttpContextFixture.CreateAuthenticated(userId));

            var result = await sut.Handle(new GetFollowersQuery(1, 20), default);

            result.IsSuccess.Should().BeTrue();
            result.Value!.TotalCount.Should().Be(1);
            result.Value.Page.Should().Be(1);
            result.Value.PageSize.Should().Be(20);
            var returned = result.Value.Items.Should().ContainSingle().Subject;
            returned.UserName.Should().Be("alice");
            returned.Identification.Should().Be("alice_id");
            returned.AvatarUrl.Should().Be("http://img");
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldSetIsFollowing_ForMutualFollowers()
        {
            const string userId = "u1";
            var items = new List<FollowViewModel>
            {
                new() { UserName = "alice" },
                new() { UserName = "bob" }
            };
            _queries.GetFollowersAsync(userId, 1, 20, null, Arg.Any<CancellationToken>()).Returns(items);
            _queries.CountFollowersAsync(userId, null, Arg.Any<CancellationToken>()).Returns(2);
            _queries.GetFollowedUserNamesAsync(userId, Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
                .Returns(new HashSet<string> { "alice" });
            var sut = new GetFollowersHandler(_queries, HttpContextFixture.CreateAuthenticated(userId));

            var result = await sut.Handle(new GetFollowersQuery(1, 20), default);

            result.Value!.Items.Single(f => f.UserName == "alice").IsFollowing.Should().BeTrue();
            result.Value.Items.Single(f => f.UserName == "bob").IsFollowing.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldPassSearchAndPageParams_ToQueries()
        {
            const string userId = "u1";
            _queries.GetFollowersAsync(userId, 2, 10, "ali", Arg.Any<CancellationToken>())
                .Returns(new List<FollowViewModel>());
            _queries.CountFollowersAsync(userId, "ali", Arg.Any<CancellationToken>()).Returns(0);
            var sut = new GetFollowersHandler(_queries, HttpContextFixture.CreateAuthenticated(userId));

            await sut.Handle(new GetFollowersQuery(2, 10, "ali"), default);

            await _queries.Received(1).GetFollowersAsync(userId, 2, 10, "ali", Arg.Any<CancellationToken>());
            await _queries.Received(1).CountFollowersAsync(userId, "ali", Arg.Any<CancellationToken>());
        }
    }
}
