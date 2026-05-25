using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Posts.Handlers;
using AskFlow.Application.Posts.Queries;
using AskFlow.Application.Posts.ViewModels;
using AskFlow.Application.Users.ViewModels;
using AskFlow.Tests.Common.Fixtures;

namespace AskFlow.Tests.Application.Posts.Handlers
{
    public class GetFollowingPostsHandlerTests
    {
        private readonly IPostQueries _postQueries = Substitute.For<IPostQueries>();
        private readonly ILikeQueries _likeQueries = Substitute.For<ILikeQueries>();

        private GetFollowingPostsHandler CreateSut(string? userId) =>
            new(_postQueries, _likeQueries,
                userId is null
                    ? HttpContextFixture.CreateUnauthenticated()
                    : HttpContextFixture.CreateAuthenticated(userId));

        [Fact]
        public async Task Handle_Unauthenticated_ShouldReturnUnauthorized()
        {
            var result = await CreateSut(null).Handle(new GetFollowingPostsQuery(), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotAuthenticated);
        }

        [Fact]
        public async Task Handle_EmptyResult_ShouldReturnEmptyPagedResult_AndSkipLikeQuery()
        {
            _postQueries.CountFollowingPostsAsync("u1", Arg.Any<CancellationToken>()).Returns(0);
            _postQueries.GetFollowingPostsAsync("u1", 1, 20, Arg.Any<CancellationToken>())
                .Returns(new List<PostsViewModel>());

            var result = await CreateSut("u1").Handle(new GetFollowingPostsQuery(), default);

            result.IsSuccess.Should().BeTrue();
            result.Value!.Items.Should().BeEmpty();
            result.Value.TotalCount.Should().Be(0);
            await _likeQueries.DidNotReceive().GetLikedPostIdsAsync(
                Arg.Any<string>(), Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldFlag_IsLiked_ForKnownPosts()
        {
            var posts = new List<PostsViewModel>
            {
                new() { Id = 1, User = new UserDto() },
                new() { Id = 2, User = new UserDto() }
            };
            _postQueries.CountFollowingPostsAsync("u1", Arg.Any<CancellationToken>()).Returns(2);
            _postQueries.GetFollowingPostsAsync("u1", 1, 20, Arg.Any<CancellationToken>()).Returns(posts);
            _likeQueries.GetLikedPostIdsAsync("u1", Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>())
                .Returns(new HashSet<int> { 2 });

            var result = await CreateSut("u1").Handle(new GetFollowingPostsQuery(), default);

            result.Value!.Items.Single(p => p.Id == 1).IsLiked.Should().BeFalse();
            result.Value.Items.Single(p => p.Id == 2).IsLiked.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldReturnPagedResult_WithCorrectPaginationFields()
        {
            _postQueries.CountFollowingPostsAsync("u1", Arg.Any<CancellationToken>()).Returns(50);
            _postQueries.GetFollowingPostsAsync("u1", 3, 5, Arg.Any<CancellationToken>())
                .Returns(new List<PostsViewModel>());

            var result = await CreateSut("u1").Handle(new GetFollowingPostsQuery(3, 5), default);

            result.IsSuccess.Should().BeTrue();
            result.Value!.TotalCount.Should().Be(50);
            result.Value.Page.Should().Be(3);
            result.Value.PageSize.Should().Be(5);
        }
    }
}
