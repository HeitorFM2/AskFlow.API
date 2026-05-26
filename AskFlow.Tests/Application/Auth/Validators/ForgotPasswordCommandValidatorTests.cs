using AskFlow.Application.Auth.Commands;
using AskFlow.Application.Common;

namespace AskFlow.Tests.Application.Auth.Validators
{
    public class ForgotPasswordCommandValidatorTests
    {
        private readonly ForgotPasswordCommandValidator _sut = new();

        [Fact]
        public void Validate_ValidPayload_ShouldPass()
        {
            var result = _sut.Validate(new ForgotPasswordCommand("user@askflow.com"));
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalido")]
        public void Validate_InvalidEmail_ShouldFail(string email)
        {
            var result = _sut.Validate(new ForgotPasswordCommand(email));
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Validate_EmptyEmail_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(new ForgotPasswordCommand(""));
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.EmailRequired);
        }

        [Fact]
        public void Validate_InvalidEmailFormat_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(new ForgotPasswordCommand("not-an-email"));
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.EmailInvalid);
        }
    }
}
