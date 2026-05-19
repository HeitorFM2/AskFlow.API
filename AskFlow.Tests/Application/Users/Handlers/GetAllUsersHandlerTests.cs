using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Users.Handlers;
using AskFlow.Application.Users.Queries;
using AskFlow.Application.Users.ViewModels;
using AskFlow.Tests.Common.Fixtures;

namespace AskFlow.Tests.Application.Users.Handlers
{
    public class GetAllUsersHandlerTests
    {
        private readonly IUserQueries _userQueries = Substitute.For<IUserQueries>();

        [Fact]
        public async Task Handle_Unauthenticated_ShouldReturnUnauthorized()
        {
            var sut = new GetAllUsersHandler(_userQueries, HttpContextFixture.CreateUnauthenticated());

            var result = await sut.Handle(new GetAllUsersQuery(), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotAuthenticated);
            await _userQueries.DidNotReceive().GetAllAsync(
                Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldReturnUsers_AndPassUserIdToExclude()
        {
            const string userId = "u1";
            var users = new List<UserDto>
            {
                new() { UserName = "alice", Identification = "alice_id", IsFollowing = false }
            };
            _userQueries.GetAllAsync(userId, null, Arg.Any<CancellationToken>()).Returns(users);
            var sut = new GetAllUsersHandler(_userQueries, HttpContextFixture.CreateAuthenticated(userId));

            var result = await sut.Handle(new GetAllUsersQuery(), default);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().ContainSingle();
            await _userQueries.Received(1).GetAllAsync(userId, null, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldForwardSearch_ToQuery()
        {
            const string userId = "u1";
            _userQueries.GetAllAsync(userId, "alice", Arg.Any<CancellationToken>())
                .Returns(new List<UserDto>());
            var sut = new GetAllUsersHandler(_userQueries, HttpContextFixture.CreateAuthenticated(userId));

            await sut.Handle(new GetAllUsersQuery("alice"), default);

            await _userQueries.Received(1).GetAllAsync(userId, "alice", Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Authenticated_EmptyResult_ShouldReturnSuccessWithEmptyList()
        {
            const string userId = "u1";
            _userQueries.GetAllAsync(userId, null, Arg.Any<CancellationToken>())
                .Returns(new List<UserDto>());
            var sut = new GetAllUsersHandler(_userQueries, HttpContextFixture.CreateAuthenticated(userId));

            var result = await sut.Handle(new GetAllUsersQuery(), default);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();
        }
    }
}
