using AskFlow.Application.Common;

namespace AskFlow.Tests.Application.Common
{
    public class PagedResultTests
    {
        [Fact]
        public void TotalPages_ShouldRoundUp_FromTotalCountAndPageSize()
        {
            var paged = new PagedResult<int>
            {
                Items = new[] { 1, 2, 3 },
                TotalCount = 23,
                Page = 1,
                PageSize = 10
            };

            paged.TotalPages.Should().Be(3);
        }

        [Fact]
        public void TotalPages_ShouldBeZero_WhenPageSizeIsZero()
        {
            var paged = new PagedResult<int>
            {
                Items = Array.Empty<int>(),
                TotalCount = 5,
                Page = 1,
                PageSize = 0
            };

            paged.TotalPages.Should().Be(0);
        }

        [Theory]
        [InlineData(1, 30, 10, false, true)]
        [InlineData(2, 30, 10, true, true)]
        [InlineData(3, 30, 10, true, false)]
        public void Pagination_Flags_ShouldReflectCurrentPage(int page, int total, int pageSize, bool hasPrev, bool hasNext)
        {
            var paged = new PagedResult<int>
            {
                Items = Array.Empty<int>(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };

            paged.HasPreviousPage.Should().Be(hasPrev);
            paged.HasNextPage.Should().Be(hasNext);
        }
    }
}
