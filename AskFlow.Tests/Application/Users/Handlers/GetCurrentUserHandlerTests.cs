using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Users.Handlers;
using AskFlow.Application.Users.Queries;
using AskFlow.Domain.Entities;
using AskFlow.Tests.Common.Builders;
using AskFlow.Tests.Common.Fixtures;
using Microsoft.AspNetCore.Identity;

namespace AskFlow.Tests.Application.Users.Handlers
{
    public class GetCurrentUserHandlerTests
    {
        private readonly UserManager<User> _userManager = UserManagerFixture.Create();
        private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();

        private GetCurrentUserHandler CreateSut() => new(_userManager, _currentUser);

        [Fact]
        public async Task Handle_Unauthenticated_ShouldReturnUnauthorized()
        {
            _currentUser.GetUserId().Returns((string?)null);

            var result = await CreateSut().Handle(new GetCurrentUserQuery(), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotAuthenticated);
            await _userManager.DidNotReceive().FindByIdAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task Handle_UserNotFound_ShouldReturnNotFound()
        {
            _currentUser.GetUserId().Returns("u1");
            _userManager.FindByIdAsync("u1").Returns((User?)null);

            var result = await CreateSut().Handle(new GetCurrentUserQuery(), default);

            result.Type.Should().Be(ResultType.NotFound);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotFound);
        }

        [Fact]
        public async Task Handle_HappyPath_ShouldReturnUserDto_WithAvatarUrl()
        {
            var user = new UserBuilder()
                .WithId("u1")
                .WithEmail("alice@example.com")
                .WithIdentification("alice")
                .WithAvatarUrl("https://blob/u1.jpg")
                .Build();
            _currentUser.GetUserId().Returns("u1");
            _userManager.FindByIdAsync("u1").Returns(user);

            var result = await CreateSut().Handle(new GetCurrentUserQuery(), default);

            result.IsSuccess.Should().BeTrue();
            result.Value!.UserName.Should().Be("alice@example.com");
            result.Value.Identification.Should().Be("alice");
            result.Value.AvatarUrl.Should().Be("https://blob/u1.jpg");
        }

        [Fact]
        public async Task Handle_UserWithoutAvatar_ShouldReturnNullAvatarUrl()
        {
            var user = new UserBuilder().WithId("u1").WithAvatarUrl(null).Build();
            _currentUser.GetUserId().Returns("u1");
            _userManager.FindByIdAsync("u1").Returns(user);

            var result = await CreateSut().Handle(new GetCurrentUserQuery(), default);

            result.IsSuccess.Should().BeTrue();
            result.Value!.AvatarUrl.Should().BeNull();
        }
    }
}
