using AskFlow.Application.Comments.Commands;

namespace AskFlow.Tests.Application.Comments.Validators
{
    public class CreateCommentCommandValidatorTests
    {
        private readonly CreateCommentCommandValidator _sut = new();

        [Fact]
        public void Validate_Valid_ShouldPass()
        {
            var result = _sut.Validate(new CreateCommentCommand(1, "ok"));
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(0, "ok")]
        [InlineData(-1, "ok")]
        [InlineData(1, "")]
        public void Validate_Invalid_ShouldFail(int postId, string content)
        {
            var result = _sut.Validate(new CreateCommentCommand(postId, content));
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Validate_TooLongContent_ShouldFail()
        {
            var result = _sut.Validate(new CreateCommentCommand(1, new string('a', 501)));
            result.IsValid.Should().BeFalse();
        }
    }
}
