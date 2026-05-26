using AskFlow.Application.Auth.Commands;
using AskFlow.Application.Auth.Handlers;
using AskFlow.Application.Common;
using AskFlow.Domain.Entities;
using AskFlow.Tests.Common.Builders;
using AskFlow.Tests.Common.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AskFlow.Tests.Application.Auth.Handlers
{
    public class ResetPasswordHandlerTests
    {
        private readonly UserManager<User> _userManager = UserManagerFixture.Create();
        private readonly ILogger<ResetPasswordHandler> _logger = Substitute.For<ILogger<ResetPasswordHandler>>();

        private ResetPasswordHandler CreateSut() => new(_userManager, _logger);

        private static ResetPasswordCommand ValidCommand(string email = "user@askflow.com") =>
            new(email, "valid-token", "NewSenha123", "NewSenha123");

        [Fact]
        public async Task Handle_WhenUserNotFound_ShouldReturnInvalidWithExpectedErrorCode()
        {
            _userManager.FindByEmailAsync("unknown@askflow.com").Returns((User?)null);

            var result = await CreateSut().Handle(ValidCommand("unknown@askflow.com"), default);

            result.Type.Should().Be(ResultType.Invalid);
            result.ErrorCode.Should().Be(ErrorCodes.AuthResetTokenInvalid);
        }

        [Fact]
        public async Task Handle_WhenResetPasswordFails_ShouldReturnInvalidWithExpectedErrorCode()
        {
            var user = new UserBuilder().WithEmail("user@askflow.com").Build();
            _userManager.FindByEmailAsync("user@askflow.com").Returns(user);
            _userManager.ResetPasswordAsync(user, "valid-token", "NewSenha123")
                .Returns(IdentityResult.Failed(new IdentityError { Code = "InvalidToken", Description = "Invalid token." }));

            var result = await CreateSut().Handle(ValidCommand(), default);

            result.Type.Should().Be(ResultType.Invalid);
            result.ErrorCode.Should().Be(ErrorCodes.AuthResetTokenInvalid);
        }

        [Fact]
        public async Task Handle_WhenResetPasswordSucceeds_ShouldReturnSuccess()
        {
            var user = new UserBuilder().WithEmail("user@askflow.com").Build();
            _userManager.FindByEmailAsync("user@askflow.com").Returns(user);
            _userManager.ResetPasswordAsync(user, "valid-token", "NewSenha123")
                .Returns(IdentityResult.Success);

            var result = await CreateSut().Handle(ValidCommand(), default);

            result.IsSuccess.Should().BeTrue();
            result.Type.Should().Be(ResultType.Ok);
        }
    }
}
