using AskFlow.Application.Auth.Commands;
using AskFlow.Application.Auth.Handlers;
using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Domain.Entities;
using AskFlow.Tests.Common.Builders;
using AskFlow.Tests.Common.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AskFlow.Tests.Application.Auth.Handlers
{
    public class LoginHandlerTests
    {
        private readonly UserManager<User> _userManager = UserManagerFixture.Create();
        private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
        private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
        private readonly ILogger<LoginHandler> _logger = Substitute.For<ILogger<LoginHandler>>();

        private LoginHandler CreateSut() => new(_userManager, _tokenService, _refreshTokens, _logger);

        [Fact]
        public async Task Handle_WhenUserNotFound_ShouldReturnUnauthorized()
        {
            _userManager.FindByEmailAsync("a@b.com").Returns((User?)null);

            var result = await CreateSut().Handle(new LoginCommand("a@b.com", "x"), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.AuthInvalidCredentials);
        }

        [Fact]
        public async Task Handle_WhenPasswordIsInvalid_ShouldReturnUnauthorized()
        {
            var user = new UserBuilder().WithEmail("a@b.com").Build();
            _userManager.FindByEmailAsync("a@b.com").Returns(user);
            _userManager.CheckPasswordAsync(user, "wrong").Returns(false);

            var result = await CreateSut().Handle(new LoginCommand("a@b.com", "wrong"), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.AuthInvalidCredentials);
        }

        [Fact]
        public async Task Handle_OnSuccess_ShouldRevokeAndIssueNewTokens()
        {
            var user = new UserBuilder().WithEmail("a@b.com").Build();
            _userManager.FindByEmailAsync("a@b.com").Returns(user);
            _userManager.CheckPasswordAsync(user, "ok").Returns(true);
            _tokenService.GenerateAccessToken(user).Returns("access");
            _tokenService.GenerateRefreshToken().Returns("refresh");
            _tokenService.RefreshTokenExpiresInDays.Returns(7);
            var expiry = DateTime.UtcNow.AddMinutes(15);
            _tokenService.GetAccessTokenExpiry().Returns(expiry);

            var result = await CreateSut().Handle(new LoginCommand("a@b.com", "ok"), default);

            result.IsSuccess.Should().BeTrue();
            result.Value!.AccessToken.Should().Be("access");
            result.Value.RefreshToken.Should().Be("refresh");
            result.Value.ExpiresAt.Should().Be(expiry);
            result.Value.User.Email.Should().Be(user.Email);
            result.Value.User.Id.Should().Be(user.Id);
            result.Value.User.Identification.Should().Be(user.Identification);
            await _refreshTokens.Received(1).RevokeAllByUserIdAsync(user.Id);
            await _refreshTokens.Received(1).AddAsync(Arg.Is<RefreshToken>(t => t.Token == "refresh"));
        }
    }
}
