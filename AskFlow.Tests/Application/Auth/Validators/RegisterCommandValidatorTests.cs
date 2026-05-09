using AskFlow.Application.Auth.Commands;
using AskFlow.Application.Common;

namespace AskFlow.Tests.Application.Auth.Validators
{
    public class RegisterCommandValidatorTests
    {
        private readonly RegisterCommandValidator _sut = new();

        [Fact]
        public void Validate_ValidPayload_ShouldPass()
        {
            var result = _sut.Validate(new RegisterCommand("user@askflow.com", "senha123", "ident"));
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("", "senha123", "ident")]
        [InlineData("invalido", "senha123", "ident")]
        [InlineData("user@askflow.com", "", "ident")]
        [InlineData("user@askflow.com", "12345", "ident")]
        [InlineData("user@askflow.com", "senha123", "")]
        public void Validate_InvalidPayload_ShouldFail(string email, string password, string identification)
        {
            var result = _sut.Validate(new RegisterCommand(email, password, identification));
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Validate_IdentificationTooLong_ShouldFail()
        {
            var longIdent = new string('a', 51);

            var result = _sut.Validate(new RegisterCommand("user@askflow.com", "senha123", longIdent));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.Identification)
                                             && e.ErrorCode == ErrorCodes.IdentificationMaxLength);
        }

        [Fact]
        public void Validate_PasswordTooShort_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(new RegisterCommand("user@askflow.com", "12345", "ident"));
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.PasswordMinLength);
        }
    }
}
