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
            var result = _sut.Validate(new RegisterCommand("user@askflow.com", "username", "Senha123", "ident"));
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("", "username", "Senha123", "ident")]
        [InlineData("invalido", "username", "Senha123", "ident")]
        [InlineData("user@askflow.com", "username", "", "ident")]
        [InlineData("user@askflow.com", "username", "Sen1a", "ident")]
        [InlineData("user@askflow.com", "username", "Senha123", "")]
        public void Validate_InvalidPayload_ShouldFail(string email, string userName, string password, string identification)
        {
            var result = _sut.Validate(new RegisterCommand(email, userName, password, identification));
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Validate_IdentificationTooLong_ShouldFail()
        {
            var longIdent = new string('a', 101);

            var result = _sut.Validate(new RegisterCommand("user@askflow.com", "username", "Senha123", longIdent));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.Identification)
                                             && e.ErrorCode == ErrorCodes.IdentificationMaxLength);
        }

        [Fact]
        public void Validate_PasswordTooShort_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(new RegisterCommand("user@askflow.com", "username", "Aa1", "ident"));
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.PasswordMinLength);
        }

        [Fact]
        public void Validate_PasswordWithoutUppercase_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(new RegisterCommand("user@askflow.com", "username", "senha123", "ident"));
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.PasswordRequiresUppercase);
        }

        [Fact]
        public void Validate_PasswordWithoutLowercase_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(new RegisterCommand("user@askflow.com", "username", "SENHA123", "ident"));
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.PasswordRequiresLowercase);
        }

        [Fact]
        public void Validate_PasswordWithoutDigit_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(new RegisterCommand("user@askflow.com", "username", "SenhaSenha", "ident"));
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.PasswordRequiresDigit);
        }
    }
}
