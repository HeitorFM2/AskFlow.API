using AskFlow.Application.Common;
using AskFlow.Application.Posts.Queries;

namespace AskFlow.Tests.Application.Posts.Validators
{
    public class GetAllPostsQueryValidatorTests
    {
        private readonly GetAllPostsQueryValidator _sut = new();

        [Fact]
        public void Validate_DefaultPaging_ShouldPass()
        {
            var result = _sut.Validate(new GetAllPostsQuery());
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 100)]
        [InlineData(5, 50)]
        public void Validate_PagingWithinBounds_ShouldPass(int page, int pageSize)
        {
            var result = _sut.Validate(new GetAllPostsQuery(page, pageSize));
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Validate_PageBelowMinimum_ShouldFail_WithExpectedErrorCode(int page)
        {
            var result = _sut.Validate(new GetAllPostsQuery(page, 20));
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllPostsQuery.Page)
                                              && e.ErrorCode == ErrorCodes.PaginationPageInvalid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(101)]
        [InlineData(1000)]
        public void Validate_PageSizeOutOfBounds_ShouldFail_WithExpectedErrorCode(int pageSize)
        {
            var result = _sut.Validate(new GetAllPostsQuery(1, pageSize));
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllPostsQuery.PageSize)
                                              && e.ErrorCode == ErrorCodes.PaginationPageSizeInvalid);
        }

        [Fact]
        public void Validate_BothPageAndPageSizeInvalid_ShouldReportBoth()
        {
            var result = _sut.Validate(new GetAllPostsQuery(0, 0));
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.PaginationPageInvalid);
            result.Errors.Should().Contain(e => e.ErrorCode == ErrorCodes.PaginationPageSizeInvalid);
        }
    }
}
