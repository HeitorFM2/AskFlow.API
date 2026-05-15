using AskFlow.Application.Auth.Commands;
using AskFlow.Application.Auth.Handlers;
using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Domain.Entities;
using AskFlow.Tests.Common.Builders;
using Microsoft.Extensions.Logging;

namespace AskFlow.Tests.Application.Auth.Handlers
{
    public class RefreshTokenHandlerTests
    {
        private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
        private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
        private readonly ILogger<RefreshTokenHandler> _logger = Substitute.For<ILogger<RefreshTokenHandler>>();

        private RefreshTokenHandler CreateSut() => new(_refreshTokens, _tokenService, _logger);

        [Fact]
        public async Task Handle_TokenNotFound_ShouldReturnUnauthorized()
        {
            _refreshTokens.GetByTokenAsync("missing").Returns((RefreshToken?)null);

            var result = await CreateSut().Handle(new RefreshTokenCommand("missing"), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.AuthRefreshTokenInvalid);
        }

        [Fact]
        public async Task Handle_TokenExpired_ShouldReturnUnauthorized()
        {
            var token = new RefreshTokenBuilder().WithExpiresAt(DateTime.UtcNow.AddDays(-1)).Build();
            _refreshTokens.GetByTokenAsync(Arg.Any<string>()).Returns(token);

            var result = await CreateSut().Handle(new RefreshTokenCommand("x"), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.AuthRefreshTokenExpiredOrRevoked);
        }

        [Fact]
        public async Task Handle_RevokedToken_ShouldDetectReuse_AndRevokeAllForUser()
        {
            var user = new UserBuilder().Build();
            var token = new RefreshTokenBuilder()
                .WithUser(user)
                .WithExpiresAt(DateTime.UtcNow.AddDays(2))
                .Revoked()
                .Build();
            _refreshTokens.GetByTokenAsync(Arg.Any<string>()).Returns(token);

            var result = await CreateSut().Handle(new RefreshTokenCommand("x"), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.AuthRefreshTokenReuseDetected);
            await _refreshTokens.Received(1).RevokeAllByUserIdAsync(user.Id);
            await _refreshTokens.DidNotReceive().AddAsync(Arg.Any<RefreshToken>());
        }

        [Fact]
        public async Task Handle_ActiveToken_ShouldRevokeOld_AndIssueNewPair()
        {
            var user = new UserBuilder().Build();
            var token = new RefreshTokenBuilder()
                .WithUser(user)
                .WithExpiresAt(DateTime.UtcNow.AddDays(2))
                .Build();
            _refreshTokens.GetByTokenAsync(Arg.Any<string>()).Returns(token);
            _tokenService.GenerateAccessToken(user).Returns("new-access");
            _tokenService.GenerateRefreshToken().Returns("new-refresh");
            _tokenService.RefreshTokenExpiresInDays.Returns(7);
            var expiry = DateTime.UtcNow.AddMinutes(15);
            _tokenService.GetAccessTokenExpiry().Returns(expiry);

            var result = await CreateSut().Handle(new RefreshTokenCommand("any"), default);

            result.IsSuccess.Should().BeTrue();
            result.Value!.AccessToken.Should().Be("new-access");
            result.Value.RefreshToken.Should().Be("new-refresh");
            result.Value.User.Id.Should().Be(user.Id);
            token.IsRevoked.Should().BeTrue();
            await _refreshTokens.Received(1).AddAsync(Arg.Is<RefreshToken>(t => t.TokenHash == RefreshToken.HashToken("new-refresh")));
        }
    }
}
