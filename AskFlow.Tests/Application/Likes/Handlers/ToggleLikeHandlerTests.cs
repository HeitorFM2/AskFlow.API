using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Likes.Commands;
using AskFlow.Application.Likes.Handlers;
using AskFlow.Tests.Common.Fixtures;

namespace AskFlow.Tests.Application.Likes.Handlers
{
    public class ToggleLikeHandlerTests
    {
        private readonly ILikeRepository _repo = Substitute.For<ILikeRepository>();

        [Fact]
        public async Task Handle_Unauthenticated_ShouldReturnUnauthorized()
        {
            var sut = new ToggleLikeHandler(_repo, HttpContextFixture.CreateUnauthenticated());

            var result = await sut.Handle(new ToggleLikeCommand(1), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotAuthenticated);
            await _repo.DidNotReceive().ToggleLikeAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldCallToggle_AndReturnTrue_WhenLiked()
        {
            const string userId = "user-1";
            _repo.ToggleLikeAsync(userId, 5, Arg.Any<CancellationToken>()).Returns(true);
            var sut = new ToggleLikeHandler(_repo, HttpContextFixture.CreateAuthenticated(userId));

            var result = await sut.Handle(new ToggleLikeCommand(5), default);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
            await _repo.Received(1).ToggleLikeAsync(userId, 5, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldCallToggle_AndReturnFalse_WhenUnliked()
        {
            const string userId = "user-1";
            _repo.ToggleLikeAsync(userId, 5, Arg.Any<CancellationToken>()).Returns(false);
            var sut = new ToggleLikeHandler(_repo, HttpContextFixture.CreateAuthenticated(userId));

            var result = await sut.Handle(new ToggleLikeCommand(5), default);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_NullUserId_ShouldReturnUnauthorized()
        {
            var sut = new ToggleLikeHandler(_repo, HttpContextFixture.CreateNullContext());

            var result = await sut.Handle(new ToggleLikeCommand(1), default);

            result.Type.Should().Be(ResultType.Unauthorized);
        }
    }
}
