using AskFlow.Application.Interfaces;
using AskFlow.Application.Posts.Handlers;
using AskFlow.Application.Posts.Queries;
using AskFlow.Application.Posts.ViewModels;
using AskFlow.Application.Users.ViewModels;
using AskFlow.Tests.Common.Fixtures;

namespace AskFlow.Tests.Application.Posts.Handlers
{
    public class GetAllPostsHandlerTests
    {
        private readonly IPostQueries _postQueries = Substitute.For<IPostQueries>();
        private readonly ILikeQueries _likeQueries = Substitute.For<ILikeQueries>();
        private readonly IFollowQueries _followQueries = Substitute.For<IFollowQueries>();

        [Fact]
        public async Task Handle_ShouldReturn_PostsViewModel_WithCounts()
        {
            var item = new PostsViewModel
            {
                Id = 1,
                Content = "content",
                CreatedAt = DateTime.UtcNow,
                Comments = 1,
                Likes = 1,
                User = new UserDto { UserName = "user", Identification = "user_a" }
            };

            _postQueries.CountAsync(Arg.Any<CancellationToken>()).Returns(1);
            _postQueries.GetAllAsync(1, 20, Arg.Any<CancellationToken>())
                .Returns(new List<PostsViewModel> { item });

            var sut = new GetAllPostsHandler(_postQueries, _likeQueries, _followQueries, HttpContextFixture.CreateUnauthenticated());

            var result = await sut.Handle(new GetAllPostsQuery(1, 20), default);

            result.IsSuccess.Should().BeTrue();
            result.Value!.TotalCount.Should().Be(1);
            var returned = result.Value.Items.Should().ContainSingle().Subject;
            returned.Id.Should().Be(1);
            returned.Comments.Should().Be(1);
            returned.Likes.Should().Be(1);
            returned.IsLiked.Should().BeFalse();
            returned.User.Identification.Should().Be("user_a");
            result.Value.Page.Should().Be(1);
            result.Value.PageSize.Should().Be(20);
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldFlag_IsLiked_ForKnownPosts()
        {
            var items = new List<PostsViewModel>
            {
                new() { Id = 1, User = new UserDto() },
                new() { Id = 2, User = new UserDto() }
            };

            _postQueries.CountAsync(Arg.Any<CancellationToken>()).Returns(2);
            _postQueries.GetAllAsync(1, 20, Arg.Any<CancellationToken>()).Returns(items);
            _likeQueries.GetLikedPostIdsAsync("u1", Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>())
                .Returns([2]);
            _followQueries.GetFollowedUserNamesAsync("u1", Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
                .Returns(new HashSet<string>());

            var sut = new GetAllPostsHandler(_postQueries, _likeQueries, _followQueries, HttpContextFixture.CreateAuthenticated("u1"));

            var result = await sut.Handle(new GetAllPostsQuery(1, 20), default);

            result.Value!.Items.Single(p => p.Id == 1).IsLiked.Should().BeFalse();
            result.Value.Items.Single(p => p.Id == 2).IsLiked.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldFlag_IsFollowing_ForFollowedAuthors()
        {
            var items = new List<PostsViewModel>
            {
                new() { Id = 1, User = new UserDto { UserName = "alice" } },
                new() { Id = 2, User = new UserDto { UserName = "bob" } }
            };

            _postQueries.CountAsync(Arg.Any<CancellationToken>()).Returns(2);
            _postQueries.GetAllAsync(1, 20, Arg.Any<CancellationToken>()).Returns(items);
            _likeQueries.GetLikedPostIdsAsync("u1", Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>())
                .Returns(new HashSet<int>());
            _followQueries.GetFollowedUserNamesAsync("u1", Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
                .Returns(new HashSet<string> { "alice" });

            var sut = new GetAllPostsHandler(_postQueries, _likeQueries, _followQueries, HttpContextFixture.CreateAuthenticated("u1"));

            var result = await sut.Handle(new GetAllPostsQuery(1, 20), default);

            result.Value!.Items.Single(p => p.Id == 1).User.IsFollowing.Should().BeTrue();
            result.Value.Items.Single(p => p.Id == 2).User.IsFollowing.Should().BeFalse();
        }
    }
}
