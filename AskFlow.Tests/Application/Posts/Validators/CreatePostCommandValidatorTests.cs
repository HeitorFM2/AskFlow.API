using AskFlow.Application.Posts.Command;

namespace AskFlow.Tests.Application.Posts.Validators
{
    public class CreatePostCommandValidatorTests
    {
        private readonly CreatePostCommandValidator _sut = new();

        [Fact]
        public void Validate_ValidContent_ShouldPass()
        {
            var result = _sut.Validate(new CreatePostCommand("hello"));
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_EmptyContent_ShouldFail()
        {
            var result = _sut.Validate(new CreatePostCommand(""));
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Validate_TooLongContent_ShouldFail()
        {
            var result = _sut.Validate(new CreatePostCommand(new string('a', 281)));
            result.IsValid.Should().BeFalse();
        }
    }
}
