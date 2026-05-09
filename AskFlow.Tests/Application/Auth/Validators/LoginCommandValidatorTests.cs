using AskFlow.Application.Auth.Commands;

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
    }
}
