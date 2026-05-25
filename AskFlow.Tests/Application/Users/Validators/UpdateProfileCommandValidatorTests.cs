using AskFlow.Application.Common;
using AskFlow.Application.Users.Commands;

namespace AskFlow.Tests.Application.Users.Validators
{
    public class UpdateProfileCommandValidatorTests
    {
        private readonly UpdateProfileCommandValidator _sut = new();

        [Fact]
        public void Validate_ValidPayload_ShouldPass()
        {
            var result = _sut.Validate(new UpdateProfileCommand("username", "my_ident"));
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("", "my_ident")]
        [InlineData("username", "")]
        public void Validate_EmptyFields_ShouldFail(string userName, string identification)
        {
            var result = _sut.Validate(new UpdateProfileCommand(userName, identification));
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Validate_EmptyUserName_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(new UpdateProfileCommand("", "ident"));
            result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateProfileCommand.UserName)
                                             && e.ErrorCode == ErrorCodes.UserNameRequired);
        }

        [Fact]
        public void Validate_EmptyIdentification_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(new UpdateProfileCommand("username", ""));
            result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateProfileCommand.Identification)
                                             && e.ErrorCode == ErrorCodes.IdentificationRequired);
        }

        [Fact]
        public void Validate_UserNameAtMaxLength_ShouldPass()
        {
            var result = _sut.Validate(new UpdateProfileCommand(new string('a', 50), "ident"));
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_UserNameTooLong_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(new UpdateProfileCommand(new string('a', 51), "ident"));
            result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateProfileCommand.UserName)
                                             && e.ErrorCode == ErrorCodes.UserNameMaxLength);
        }

        [Fact]
        public void Validate_IdentificationAtMaxLength_ShouldPass()
        {
            var result = _sut.Validate(new UpdateProfileCommand("username", new string('a', 50)));
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_IdentificationTooLong_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(new UpdateProfileCommand("username", new string('a', 51)));
            result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateProfileCommand.Identification)
                                             && e.ErrorCode == ErrorCodes.IdentificationMaxLength);
        }
    }
}
