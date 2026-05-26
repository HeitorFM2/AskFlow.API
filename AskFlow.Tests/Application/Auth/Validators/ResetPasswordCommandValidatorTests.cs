using AskFlow.Application.Auth.Commands;
using AskFlow.Application.Common;

namespace AskFlow.Tests.Application.Auth.Validators
{
    public class ResetPasswordCommandValidatorTests
    {
        private readonly ResetPasswordCommandValidator _sut = new();

        private static ResetPasswordCommand Valid() =>
            new("user@askflow.com", "valid-token", "Senha123", "Senha123");

        [Fact]
        public void Validate_ValidPayload_ShouldPass()
        {
            var result = _sut.Validate(Valid());
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("", "token", "Senha123", "Senha123")]
        [InlineData("invalido", "token", "Senha123", "Senha123")]
        [InlineData("user@askflow.com", "", "Senha123", "Senha123")]
        [InlineData("user@askflow.com", "token", "", "")]
        [InlineData("user@askflow.com", "token", "Senha123", "Diferente1")]
        public void Validate_InvalidPayload_ShouldFail(string email, string token, string newPassword, string confirmPassword)
        {
            var result = _sut.Validate(new ResetPasswordCommand(email, token, newPassword, confirmPassword));
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Validate_EmptyEmail_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(Valid() with { Email = "" });
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.EmailRequired);
        }

        [Fact]
        public void Validate_InvalidEmailFormat_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(Valid() with { Email = "not-an-email" });
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.EmailInvalid);
        }

        [Fact]
        public void Validate_EmptyToken_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(Valid() with { Token = "" });
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.ResetTokenRequired);
        }

        [Fact]
        public void Validate_EmptyPassword_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(Valid() with { NewPassword = "", ConfirmPassword = "" });
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.PasswordRequired);
        }

        [Fact]
        public void Validate_PasswordTooShort_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(Valid() with { NewPassword = "Aa1", ConfirmPassword = "Aa1" });
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.PasswordMinLength);
        }

        [Fact]
        public void Validate_PasswordWithoutUppercase_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(Valid() with { NewPassword = "senha123", ConfirmPassword = "senha123" });
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.PasswordRequiresUppercase);
        }

        [Fact]
        public void Validate_PasswordWithoutLowercase_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(Valid() with { NewPassword = "SENHA123", ConfirmPassword = "SENHA123" });
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.PasswordRequiresLowercase);
        }

        [Fact]
        public void Validate_PasswordWithoutDigit_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(Valid() with { NewPassword = "SenhaSenha", ConfirmPassword = "SenhaSenha" });
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.PasswordRequiresDigit);
        }

        [Fact]
        public void Validate_EmptyConfirmPassword_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(Valid() with { ConfirmPassword = "" });
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.PasswordConfirmRequired);
        }

        [Fact]
        public void Validate_PasswordMismatch_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(Valid() with { ConfirmPassword = "Diferente1" });
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.PasswordConfirmMismatch);
        }
    }
}
