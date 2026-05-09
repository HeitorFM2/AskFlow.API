using AskFlow.Application.Auth.Commands;

namespace AskFlow.Tests.Application.Auth.Validators
{
    public class RefreshTokenCommandValidatorTests
    {
        private readonly RefreshTokenCommandValidator _sut = new();

        [Fact]
        public void Validate_NotEmpty_ShouldPass()
        {
            var result = _sut.Validate(new RefreshTokenCommand("token"));
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_Empty_ShouldFail()
        {
            var result = _sut.Validate(new RefreshTokenCommand(""));
            result.IsValid.Should().BeFalse();
        }
    }
}
