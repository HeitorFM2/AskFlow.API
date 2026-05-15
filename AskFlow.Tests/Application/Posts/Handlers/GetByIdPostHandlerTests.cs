using AskFlow.Application.Comments.ViewModels;
using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Posts.Handlers;
using AskFlow.Application.Posts.Queries;
using AskFlow.Application.Posts.ViewModels;
using AskFlow.Application.Users.ViewModels;
using AskFlow.Tests.Common.Fixtures;

namespace AskFlow.Tests.Application.Posts.Handlers
{
    public class GetByIdPostHandlerTests
    {
        private readonly IPostQueries _postQueries = Substitute.For<IPostQueries>();
        private readonly ILikeQueries _likeQueries = Substitute.For<ILikeQueries>();

        [Fact]
        public async Task Handle_NotFound_ShouldReturnNotFound()
        {
            _postQueries.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((PostViewModel?)null);
            var sut = new GetByIdPostHandler(_postQueries, _likeQueries, HttpContextFixture.CreateAuthenticated("u1"));

            var result = await sut.Handle(new GetByIdPostQuery(1), default);

            result.Type.Should().Be(ResultType.NotFound);
            result.ErrorCode.Should().Be(ErrorCodes.PostNotFound);
        }

        [Fact]
        public async Task Handle_PostFound_ShouldReturnViewModel_IncludingComments()
        {
            var user = new UserDto { UserName = "user", Identification = "ident" };
            var comment = new CommentViewModel
            {
                Id = 20,
                Content = "comment",
                CreatedAt = DateTime.UtcNow,
                ReplyCount = 1,
                User = user
            };
            var post = new PostViewModel
            {
                Id = 7,
                Content = "post content",
                CreatedAt = DateTime.UtcNow,
                Likes = 1,
                Comments = [comment],
                User = user
            };

            _postQueries.GetByIdAsync(7, Arg.Any<CancellationToken>()).Returns(post);
            _likeQueries.GetLikedPostIdsAsync("u1", Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>())
                .Returns([7]);

            var sut = new GetByIdPostHandler(_postQueries, _likeQueries, HttpContextFixture.CreateAuthenticated("u1"));

            var result = await sut.Handle(new GetByIdPostQuery(7), default);

            result.IsSuccess.Should().BeTrue();
            result.Value!.Id.Should().Be(7);
            result.Value.Likes.Should().Be(1);
            result.Value.IsLiked.Should().BeTrue();
            result.Value.Comments.Should().ContainSingle().Which.ReplyCount.Should().Be(1);
            result.Value.User.Identification.Should().Be("ident");
        }

        [Fact]
        public async Task Handle_Unauthenticated_ShouldStillReturnPost_WithIsLikedFalse()
        {
            var post = new PostViewModel
            {
                Id = 7,
                Content = "x",
                CreatedAt = DateTime.UtcNow,
                Comments = [],
                User = new UserDto()
            };
            _postQueries.GetByIdAsync(7, Arg.Any<CancellationToken>()).Returns(post);
            var sut = new GetByIdPostHandler(_postQueries, _likeQueries, HttpContextFixture.CreateUnauthenticated());

            var result = await sut.Handle(new GetByIdPostQuery(7), default);

            result.IsSuccess.Should().BeTrue();
            result.Value!.IsLiked.Should().BeFalse();
            await _likeQueries.DidNotReceive().GetLikedPostIdsAsync(Arg.Any<string>(), Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>());
        }
    }
}
