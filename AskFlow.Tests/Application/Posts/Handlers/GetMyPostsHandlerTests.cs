using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Posts.Handlers;
using AskFlow.Application.Posts.Queries;
using AskFlow.Application.Posts.ViewModels;
using AskFlow.Application.Users.ViewModels;
using AskFlow.Tests.Common.Fixtures;

namespace AskFlow.Tests.Application.Posts.Handlers
{
    public class GetMyPostsHandlerTests
    {
        private readonly IPostQueries _postQueries = Substitute.For<IPostQueries>();
        private readonly ILikeQueries _likeQueries = Substitute.For<ILikeQueries>();

        private GetMyPostsHandler CreateSut(string? userId) =>
            new(_postQueries, _likeQueries,
                userId is null
                    ? HttpContextFixture.CreateUnauthenticated()
                    : HttpContextFixture.CreateAuthenticated(userId));

        [Fact]
        public async Task Handle_Unauthenticated_ShouldReturnUnauthorized()
        {
            var sut = CreateSut(null);

            var result = await sut.Handle(new GetMyPostsQuery(), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotAuthenticated);
            await _postQueries.DidNotReceive().GetByUserAsync(
                Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldReturnPagedResult_WithCorrectFields()
        {
            const string userId = "user-1";
            var item = new PostsViewModel
            {
                Id = 5,
                Content = "my post",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Comments = 2,
                Likes = 3,
                User = new UserDto { UserName = "john", Identification = "john_doe" }
            };

            _postQueries.CountByUserAsync(userId, Arg.Any<CancellationToken>()).Returns(1);
            _postQueries.GetByUserAsync(userId, 1, 20, Arg.Any<CancellationToken>())
                .Returns(new List<PostsViewModel> { item });
            _likeQueries.GetLikedPostIdsAsync(userId, Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>())
                .Returns([5]);

            var result = await CreateSut(userId).Handle(new GetMyPostsQuery(1, 20), default);

            result.IsSuccess.Should().BeTrue();
            result.Value!.TotalCount.Should().Be(1);
            result.Value.Page.Should().Be(1);
            result.Value.PageSize.Should().Be(20);

            var returned = result.Value.Items.Should().ContainSingle().Subject;
            returned.Id.Should().Be(5);
            returned.Content.Should().Be("my post");
            returned.Comments.Should().Be(2);
            returned.Likes.Should().Be(3);
            returned.IsLiked.Should().BeTrue();
            returned.User.UserName.Should().Be("john");
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldFlag_IsLiked_Correctly()
        {
            const string userId = "user-1";
            var items = new List<PostsViewModel>
            {
                new() { Id = 1, User = new UserDto() },
                new() { Id = 2, User = new UserDto() },
                new() { Id = 3, User = new UserDto() }
            };

            _postQueries.CountByUserAsync(userId, Arg.Any<CancellationToken>()).Returns(3);
            _postQueries.GetByUserAsync(userId, 1, 20, Arg.Any<CancellationToken>()).Returns(items);
            _likeQueries.GetLikedPostIdsAsync(userId, Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>())
                .Returns([2]);

            var result = await CreateSut(userId).Handle(new GetMyPostsQuery(1, 20), default);

            result.Value!.Items.Single(p => p.Id == 1).IsLiked.Should().BeFalse();
            result.Value.Items.Single(p => p.Id == 2).IsLiked.Should().BeTrue();
            result.Value.Items.Single(p => p.Id == 3).IsLiked.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_Authenticated_EmptyPosts_ShouldReturnEmptyPagedResult_WithoutCallingLikeQueries()
        {
            const string userId = "user-1";
            _postQueries.CountByUserAsync(userId, Arg.Any<CancellationToken>()).Returns(0);
            _postQueries.GetByUserAsync(userId, 1, 20, Arg.Any<CancellationToken>())
                .Returns(new List<PostsViewModel>());

            var result = await CreateSut(userId).Handle(new GetMyPostsQuery(1, 20), default);

            result.IsSuccess.Should().BeTrue();
            result.Value!.Items.Should().BeEmpty();
            result.Value.TotalCount.Should().Be(0);
            await _likeQueries.DidNotReceive().GetLikedPostIdsAsync(
                Arg.Any<string>(), Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldPassCorrectUserId_ToQueries()
        {
            const string userId = "user-42";
            _postQueries.CountByUserAsync(userId, Arg.Any<CancellationToken>()).Returns(0);
            _postQueries.GetByUserAsync(userId, 1, 20, Arg.Any<CancellationToken>())
                .Returns(new List<PostsViewModel>());

            await CreateSut(userId).Handle(new GetMyPostsQuery(1, 20), default);

            await _postQueries.Received(1).GetByUserAsync(userId, 1, 20, Arg.Any<CancellationToken>());
            await _postQueries.Received(1).CountByUserAsync(userId, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldPassPageParams_ToQueries()
        {
            const string userId = "user-1";
            _postQueries.CountByUserAsync(userId, Arg.Any<CancellationToken>()).Returns(0);
            _postQueries.GetByUserAsync(userId, 2, 5, Arg.Any<CancellationToken>())
                .Returns(new List<PostsViewModel>());

            await CreateSut(userId).Handle(new GetMyPostsQuery(2, 5), default);

            await _postQueries.Received(1).GetByUserAsync(userId, 2, 5, Arg.Any<CancellationToken>());
        }
    }
}
