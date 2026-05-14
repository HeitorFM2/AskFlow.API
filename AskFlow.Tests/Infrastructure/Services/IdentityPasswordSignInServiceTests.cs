using AskFlow.Application.Interfaces;
using AskFlow.Domain.Entities;
using AskFlow.Infrastructure.Services;
using AskFlow.Tests.Common.Builders;
using AskFlow.Tests.Common.Fixtures;
using Microsoft.AspNetCore.Identity;

namespace AskFlow.Tests.Infrastructure.Services
{
    public class IdentityPasswordSignInServiceTests
    {
        private readonly UserManager<User> _userManager = UserManagerFixture.Create();
        private readonly SignInManager<User> _signInManager;

        public IdentityPasswordSignInServiceTests()
        {
            _signInManager = SignInManagerFixture.Create(_userManager);
        }

        private IdentityPasswordSignInService CreateSut() => new(_signInManager);

        [Fact]
        public async Task CheckPasswordAsync_WhenSucceeded_ShouldReturnSuccess()
        {
            var user = new UserBuilder().Build();
            _signInManager.CheckPasswordSignInAsync(user, "pwd", true).Returns(SignInResult.Success);

            var result = await CreateSut().CheckPasswordAsync(user, "pwd", default);

            result.Should().Be(PasswordSignInResult.Success);
        }

        [Fact]
        public async Task CheckPasswordAsync_WhenLockedOut_ShouldReturnLockedOut()
        {
            var user = new UserBuilder().Build();
            _signInManager.CheckPasswordSignInAsync(user, "pwd", true).Returns(SignInResult.LockedOut);

            var result = await CreateSut().CheckPasswordAsync(user, "pwd", default);

            result.Should().Be(PasswordSignInResult.LockedOut);
        }

        [Fact]
        public async Task CheckPasswordAsync_WhenFailed_ShouldReturnFailed()
        {
            var user = new UserBuilder().Build();
            _signInManager.CheckPasswordSignInAsync(user, "pwd", true).Returns(SignInResult.Failed);

            var result = await CreateSut().CheckPasswordAsync(user, "pwd", default);

            result.Should().Be(PasswordSignInResult.Failed);
        }

        [Fact]
        public async Task CheckPasswordAsync_WhenNotAllowed_ShouldReturnFailed()
        {
            var user = new UserBuilder().Build();
            _signInManager.CheckPasswordSignInAsync(user, "pwd", true).Returns(SignInResult.NotAllowed);

            var result = await CreateSut().CheckPasswordAsync(user, "pwd", default);

            result.Should().Be(PasswordSignInResult.Failed);
        }

        [Fact]
        public async Task CheckPasswordAsync_WhenTwoFactorRequired_ShouldReturnFailed()
        {
            var user = new UserBuilder().Build();
            _signInManager.CheckPasswordSignInAsync(user, "pwd", true).Returns(SignInResult.TwoFactorRequired);

            var result = await CreateSut().CheckPasswordAsync(user, "pwd", default);

            result.Should().Be(PasswordSignInResult.Failed);
        }

        [Fact]
        public async Task CheckPasswordAsync_ShouldDelegate_WithLockoutOnFailureTrue()
        {
            var user = new UserBuilder().Build();
            _signInManager.CheckPasswordSignInAsync(user, "pwd", true).Returns(SignInResult.Success);

            await CreateSut().CheckPasswordAsync(user, "pwd", default);

            await _signInManager.Received(1).CheckPasswordSignInAsync(user, "pwd", true);
        }
    }
}
