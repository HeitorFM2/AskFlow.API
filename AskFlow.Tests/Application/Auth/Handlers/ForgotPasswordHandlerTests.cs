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
    public class ForgotPasswordHandlerTests
    {
        private readonly UserManager<User> _userManager = UserManagerFixture.Create();
        private readonly IEmailService _emailService = Substitute.For<IEmailService>();
        private readonly ILogger<ForgotPasswordHandler> _logger = Substitute.For<ILogger<ForgotPasswordHandler>>();

        private ForgotPasswordHandler CreateSut() => new(_userManager, _emailService, _logger);

        [Fact]
        public async Task Handle_WhenUserNotFound_ShouldReturnSuccessWithoutSendingEmail()
        {
            _userManager.FindByEmailAsync("unknown@askflow.com").Returns((User?)null);

            var result = await CreateSut().Handle(new ForgotPasswordCommand("unknown@askflow.com"), default);

            result.IsSuccess.Should().BeTrue();
            await _emailService.DidNotReceive().SendPasswordResetEmailAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WhenUserExists_ShouldGenerateTokenAndSendEmail()
        {
            var user = new UserBuilder().WithEmail("user@askflow.com").Build();
            _userManager.FindByEmailAsync("user@askflow.com").Returns(user);
            _userManager.GeneratePasswordResetTokenAsync(user).Returns("reset-token");

            var result = await CreateSut().Handle(new ForgotPasswordCommand("user@askflow.com"), default);

            result.IsSuccess.Should().BeTrue();
            await _emailService.Received(1).SendPasswordResetEmailAsync(
                "user@askflow.com", "reset-token", Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WhenUserExists_ShouldReturnSuccess()
        {
            var user = new UserBuilder().WithEmail("user@askflow.com").Build();
            _userManager.FindByEmailAsync("user@askflow.com").Returns(user);
            _userManager.GeneratePasswordResetTokenAsync(user).Returns("reset-token");

            var result = await CreateSut().Handle(new ForgotPasswordCommand("user@askflow.com"), default);

            result.Type.Should().Be(ResultType.Ok);
        }
    }
}
