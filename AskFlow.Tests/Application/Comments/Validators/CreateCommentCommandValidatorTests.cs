using AskFlow.Application.Comments.Commands;
using AskFlow.Application.Common;

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
        public void Validate_PostIdInvalid_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(new CreateCommentCommand(0, "ok"));
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.CommentPostIdInvalid);
        }

        [Fact]
        public void Validate_EmptyContent_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(new CreateCommentCommand(1, ""));
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.CommentContentRequired);
        }

        [Fact]
        public void Validate_TooLongContent_ShouldHaveExpectedErrorCode()
        {
            var result = _sut.Validate(new CreateCommentCommand(1, new string('a', 501)));
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.CommentContentMaxLength);
        }
    }
}
