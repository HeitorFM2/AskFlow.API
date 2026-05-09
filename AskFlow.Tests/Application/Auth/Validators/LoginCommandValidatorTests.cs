using AskFlow.Application.Auth.Commands;
using AskFlow.Application.Common;

namespace AskFlow.Tests.Application.Auth.Validators
{
    public class LoginCommandValidatorTests
    {
        private readonly LoginCommandValidator _sut = new();

        [Fact]
        public void Validate_ValidPayload_ShouldPass()
        {
            var result = _sut.Validate(new LoginCommand("user@askflow.com", "senha123"));
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("", "senha")]
        [InlineData("invalido", "senha")]
        [InlineData("user@askflow.com", "")]
        public void Validate_InvalidPayload_ShouldFail(string email, string password)
        {
            var result = _sut.Validate(new LoginCommand(email, password));
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Validate_EmptyEmail_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(new LoginCommand("", "x"));
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.EmailRequired);
        }

        [Fact]
        public void Validate_InvalidEmail_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(new LoginCommand("not-email", "x"));
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.EmailInvalid);
        }

        [Fact]
        public void Validate_EmptyPassword_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(new LoginCommand("user@askflow.com", ""));
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.PasswordRequired);
        }
    }
}
