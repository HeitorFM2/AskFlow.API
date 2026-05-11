using AskFlow.Application.Auth.Commands;
using AskFlow.Application.Auth.Handlers;
using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Domain.Entities;
using AskFlow.Tests.Common.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AskFlow.Tests.Application.Auth.Handlers
{
    public class RegisterHandlerTests
    {
        private readonly UserManager<User> _userManager = UserManagerFixture.Create();
        private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
        private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
        private readonly ILogger<RegisterHandler> _logger = Substitute.For<ILogger<RegisterHandler>>();

        private RegisterHandler CreateSut() => new(_userManager, _tokenService, _refreshTokens, _logger);

        [Fact]
        public async Task Handle_WhenIdentityFails_ShouldReturnInvalid_WithJoinedErrors()
        {
            _userManager.CreateAsync(Arg.Any<User>(), Arg.Any<string>())
                .Returns(IdentityResult.Failed(
                    new IdentityError { Description = "Senha fraca" },
                    new IdentityError { Description = "Email já em uso" }));

            var result = await CreateSut().Handle(new RegisterCommand("a@b.com", "1", "id"), default);

            result.Type.Should().Be(ResultType.Invalid);
            result.Error.Should().Contain("Senha fraca").And.Contain("Email já em uso");
        }

        [Fact]
        public async Task Handle_WhenIdentitySucceeds_ShouldGenerateTokens_AndPersistRefreshToken()
        {
            _userManager.CreateAsync(Arg.Any<User>(), "pwd").Returns(IdentityResult.Success);
            _tokenService.GenerateAccessToken(Arg.Any<User>()).Returns("acc");
            _tokenService.GenerateRefreshToken().Returns("ref");
            _tokenService.RefreshTokenExpiresInDays.Returns(5);
            var expiry = DateTime.UtcNow.AddMinutes(20);
            _tokenService.GetAccessTokenExpiry().Returns(expiry);

            var result = await CreateSut().Handle(new RegisterCommand("a@b.com", "pwd", "id"), default);

            result.IsSuccess.Should().BeTrue();
            result.Value!.AccessToken.Should().Be("acc");
            result.Value.RefreshToken.Should().Be("ref");
            result.Value.ExpiresAt.Should().Be(expiry);
            result.Value.User.Email.Should().Be("a@b.com");
            result.Value.User.Identification.Should().Be("id");
            await _refreshTokens.Received(1).AddAsync(Arg.Is<RefreshToken>(t => t.TokenHash == RefreshToken.HashToken("ref")));
        }
    }
}
