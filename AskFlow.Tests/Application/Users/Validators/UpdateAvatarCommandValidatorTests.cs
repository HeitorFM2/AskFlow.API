using AskFlow.Application.Common;
using AskFlow.Application.Users.Commands;

namespace AskFlow.Tests.Application.Users.Validators
{
    public class UpdateAvatarCommandValidatorTests
    {
        private readonly UpdateAvatarCommandValidator _sut = new();

        private static Stream NonEmptyStream() => new MemoryStream([1, 2, 3]);

        [Theory]
        [InlineData("image/jpeg")]
        [InlineData("image/png")]
        [InlineData("image/webp")]
        public void Validate_ValidPayload_ShouldPass(string contentType)
        {
            var result = _sut.Validate(new UpdateAvatarCommand(NonEmptyStream(), contentType, 1024));
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ZeroLength_ShouldFail_WithAvatarRequired()
        {
            var result = _sut.Validate(new UpdateAvatarCommand(NonEmptyStream(), "image/jpeg", 0));
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.AvatarRequired);
        }

        [Fact]
        public void Validate_OverMaxSize_ShouldFail_WithAvatarTooLarge()
        {
            var result = _sut.Validate(new UpdateAvatarCommand(NonEmptyStream(), "image/jpeg", (2 * 1024 * 1024) + 1));
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.AvatarTooLarge);
        }

        [Fact]
        public void Validate_ExactlyMaxSize_ShouldPass()
        {
            var result = _sut.Validate(new UpdateAvatarCommand(NonEmptyStream(), "image/jpeg", 2 * 1024 * 1024));
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("image/gif")]
        [InlineData("application/pdf")]
        [InlineData("text/plain")]
        [InlineData("")]
        public void Validate_DisallowedContentType_ShouldFail_WithAvatarContentTypeInvalid(string contentType)
        {
            var result = _sut.Validate(new UpdateAvatarCommand(NonEmptyStream(), contentType, 1024));
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.AvatarContentTypeInvalid);
        }
    }
}
