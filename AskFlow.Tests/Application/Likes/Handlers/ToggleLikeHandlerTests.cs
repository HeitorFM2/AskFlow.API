using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Likes.Commands;
using AskFlow.Application.Likes.Handlers;
using AskFlow.Domain.Entities;
using AskFlow.Tests.Common.Builders;
using AskFlow.Tests.Common.Fixtures;

namespace AskFlow.Tests.Application.Likes.Handlers
{
    public class ToggleLikeHandlerTests
    {
        private readonly ILikeRepository _likes = Substitute.For<ILikeRepository>();
        private readonly IPostRepository _posts = Substitute.For<IPostRepository>();

        [Fact]
        public async Task Handle_Unauthenticated_ShouldReturnUnauthorized()
        {
            var sut = new ToggleLikeHandler(_likes, _posts, HttpContextFixture.CreateUnauthenticated());

            var result = await sut.Handle(new ToggleLikeCommand(1), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotAuthenticated);
            await _likes.DidNotReceive().ToggleLikeAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_PostNotFound_ShouldReturnNotFound()
        {
            _posts.FindByIdAsync(5).Returns((Post?)null);
            var sut = new ToggleLikeHandler(_likes, _posts, HttpContextFixture.CreateAuthenticated("u1"));

            var result = await sut.Handle(new ToggleLikeCommand(5), default);

            result.Type.Should().Be(ResultType.NotFound);
            result.ErrorCode.Should().Be(ErrorCodes.PostNotFound);
            await _likes.DidNotReceive().ToggleLikeAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldCallToggle_AndReturnTrue_WhenLiked()
        {
            const string userId = "u1";
            _posts.FindByIdAsync(5).Returns(new PostBuilder().WithId(5).Build());
            _likes.ToggleLikeAsync(userId, 5, Arg.Any<CancellationToken>()).Returns(true);
            var sut = new ToggleLikeHandler(_likes, _posts, HttpContextFixture.CreateAuthenticated(userId));

            var result = await sut.Handle(new ToggleLikeCommand(5), default);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
            await _likes.Received(1).ToggleLikeAsync(userId, 5, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldCallToggle_AndReturnFalse_WhenUnliked()
        {
            const string userId = "u1";
            _posts.FindByIdAsync(5).Returns(new PostBuilder().WithId(5).Build());
            _likes.ToggleLikeAsync(userId, 5, Arg.Any<CancellationToken>()).Returns(false);
            var sut = new ToggleLikeHandler(_likes, _posts, HttpContextFixture.CreateAuthenticated(userId));

            var result = await sut.Handle(new ToggleLikeCommand(5), default);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeFalse();
        }
    }
}
