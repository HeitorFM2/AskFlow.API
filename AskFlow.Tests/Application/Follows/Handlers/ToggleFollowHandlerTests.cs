using AskFlow.Application.Common;
using AskFlow.Application.Follows.Commands;
using AskFlow.Application.Follows.Handlers;
using AskFlow.Application.Interfaces;
using AskFlow.Domain.Entities;
using AskFlow.Tests.Common.Builders;
using AskFlow.Tests.Common.Fixtures;
using Microsoft.AspNetCore.Identity;

namespace AskFlow.Tests.Application.Follows.Handlers
{
    public class ToggleFollowHandlerTests
    {
        private readonly IFollowRepository _repository = Substitute.For<IFollowRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly UserManager<User> _userManager = UserManagerFixture.Create();

        [Fact]
        public async Task Handle_Unauthenticated_ShouldReturnUnauthorized()
        {
            var sut = new ToggleFollowHandler(_repository, _unitOfWork, HttpContextFixture.CreateUnauthenticated(), _userManager);

            var result = await sut.Handle(new ToggleFollowCommand("alice"), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotAuthenticated);
            await _repository.DidNotReceive().ToggleAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_TargetUserNotFound_ShouldReturnNotFound()
        {
            _userManager.FindByNameAsync("ghost").Returns((User?)null);
            var sut = new ToggleFollowHandler(_repository, _unitOfWork, HttpContextFixture.CreateAuthenticated("u1"), _userManager);

            var result = await sut.Handle(new ToggleFollowCommand("ghost"), default);

            result.Type.Should().Be(ResultType.NotFound);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotFound);
            await _repository.DidNotReceive().ToggleAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_FollowSelf_ShouldReturnInvalid()
        {
            var user = new UserBuilder().WithId("u1").Build();
            _userManager.FindByNameAsync(user.UserName!).Returns(user);
            var sut = new ToggleFollowHandler(_repository, _unitOfWork, HttpContextFixture.CreateAuthenticated("u1"), _userManager);

            var result = await sut.Handle(new ToggleFollowCommand(user.UserName!), default);

            result.Type.Should().Be(ResultType.Invalid);
            result.ErrorCode.Should().Be(ErrorCodes.FollowCannotFollowSelf);
            await _repository.DidNotReceive().ToggleAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ValidFollow_ShouldCallToggle_AndReturnTrue()
        {
            var target = new UserBuilder().WithId("u2").Build();
            _userManager.FindByNameAsync(target.UserName!).Returns(target);
            _repository.ToggleAsync("u1", "u2", Arg.Any<CancellationToken>()).Returns(true);
            var sut = new ToggleFollowHandler(_repository, _unitOfWork, HttpContextFixture.CreateAuthenticated("u1"), _userManager);

            var result = await sut.Handle(new ToggleFollowCommand(target.UserName!), default);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
            await _repository.Received(1).ToggleAsync("u1", "u2", Arg.Any<CancellationToken>());
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ValidUnfollow_ShouldCallToggle_AndReturnFalse()
        {
            var target = new UserBuilder().WithId("u2").Build();
            _userManager.FindByNameAsync(target.UserName!).Returns(target);
            _repository.ToggleAsync("u1", "u2", Arg.Any<CancellationToken>()).Returns(false);
            var sut = new ToggleFollowHandler(_repository, _unitOfWork, HttpContextFixture.CreateAuthenticated("u1"), _userManager);

            var result = await sut.Handle(new ToggleFollowCommand(target.UserName!), default);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeFalse();
        }
    }
}
