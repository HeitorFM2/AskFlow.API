using AskFlow.Application.Common;
using AskFlow.Application.Follows.Handlers;
using AskFlow.Application.Follows.Queries;
using AskFlow.Application.Follows.ViewModels;
using AskFlow.Application.Interfaces;
using AskFlow.Tests.Common.Fixtures;

namespace AskFlow.Tests.Application.Follows.Handlers
{
    public class GetFollowingHandlerTests
    {
        private readonly IFollowQueries _queries = Substitute.For<IFollowQueries>();

        [Fact]
        public async Task Handle_Unauthenticated_ShouldReturnUnauthorized()
        {
            var sut = new GetFollowingHandler(_queries, HttpContextFixture.CreateUnauthenticated());

            var result = await sut.Handle(new GetFollowingQuery(), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotAuthenticated);
            await _queries.DidNotReceive().GetFollowingAsync(
                Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldReturnPagedResult_WithMappedFields()
        {
            const string userId = "u1";
            var item = new FollowViewModel { UserName = "bob", Identification = "bob_id", AvatarUrl = "http://img" };
            _queries.GetFollowingAsync(userId, 1, 20, null, Arg.Any<CancellationToken>())
                .Returns(new List<FollowViewModel> { item });
            _queries.CountFollowingAsync(userId, null, Arg.Any<CancellationToken>()).Returns(1);
            var sut = new GetFollowingHandler(_queries, HttpContextFixture.CreateAuthenticated(userId));

            var result = await sut.Handle(new GetFollowingQuery(1, 20), default);

            result.IsSuccess.Should().BeTrue();
            result.Value!.TotalCount.Should().Be(1);
            result.Value.Page.Should().Be(1);
            result.Value.PageSize.Should().Be(20);
            var returned = result.Value.Items.Should().ContainSingle().Subject;
            returned.UserName.Should().Be("bob");
            returned.Identification.Should().Be("bob_id");
            returned.AvatarUrl.Should().Be("http://img");
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldPassSearchAndPageParams_ToQueries()
        {
            const string userId = "u1";
            _queries.GetFollowingAsync(userId, 3, 5, "bob", Arg.Any<CancellationToken>())
                .Returns(new List<FollowViewModel>());
            _queries.CountFollowingAsync(userId, "bob", Arg.Any<CancellationToken>()).Returns(0);
            var sut = new GetFollowingHandler(_queries, HttpContextFixture.CreateAuthenticated(userId));

            await sut.Handle(new GetFollowingQuery(3, 5, "bob"), default);

            await _queries.Received(1).GetFollowingAsync(userId, 3, 5, "bob", Arg.Any<CancellationToken>());
            await _queries.Received(1).CountFollowingAsync(userId, "bob", Arg.Any<CancellationToken>());
        }
    }
}
