using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Likes.Dtos;
using AskFlow.Application.Likes.Handlers;
using AskFlow.Application.Likes.Queries;
using AskFlow.Tests.Common.Fixtures;

namespace AskFlow.Tests.Application.Likes.Handlers
{
    public class GetLikedPostsHandlerTests
    {
        private readonly ILikeRepository _repo = Substitute.For<ILikeRepository>();

        [Fact]
        public async Task Handle_Unauthenticated_ShouldReturnUnauthorized()
        {
            var sut = new GetLikedPostsHandler(_repo, HttpContextFixture.CreateUnauthenticated());

            var result = await sut.Handle(new GetLikedPostsQuery(), default);

            result.Type.Should().Be(ResultType.Unauthorized);
            result.ErrorCode.Should().Be(ErrorCodes.UserNotAuthenticated);
            await _repo.DidNotReceive().GetLikePostsAsync(
                Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldReturnPagedResult_WithMappedFields()
        {
            const string userId = "user-1";
            var dto = new LikedPostDto
            {
                PostId = 10,
                Content = "post content",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CommentsCount = 3,
                LikesCount = 7,
                AuthorUserName = "john",
                AuthorIdentification = "john_doe"
            };

            _repo.CountLikedPostAsync(userId, Arg.Any<CancellationToken>()).Returns(1);
            _repo.GetLikePostsAsync(userId, 1, 20, Arg.Any<CancellationToken>())
                .Returns(new List<LikedPostDto> { dto });

            var sut = new GetLikedPostsHandler(_repo, HttpContextFixture.CreateAuthenticated(userId));
            var result = await sut.Handle(new GetLikedPostsQuery(1, 20), default);

            result.IsSuccess.Should().BeTrue();
            result.Value!.TotalCount.Should().Be(1);
            result.Value.Page.Should().Be(1);
            result.Value.PageSize.Should().Be(20);

            var item = result.Value.Items.Should().ContainSingle().Subject;
            item.Id.Should().Be(10);
            item.Content.Should().Be("post content");
            item.CreatedAt.Should().Be(dto.CreatedAt);
            item.Comments.Should().Be(3);
            item.Likes.Should().Be(7);
            item.IsLiked.Should().BeTrue();
            item.User.UserName.Should().Be("john");
            item.User.Identification.Should().Be("john_doe");
        }

        [Fact]
        public async Task Handle_Authenticated_EmptyLikes_ShouldReturnEmptyPagedResult()
        {
            const string userId = "user-1";
            _repo.CountLikedPostAsync(userId, Arg.Any<CancellationToken>()).Returns(0);
            _repo.GetLikePostsAsync(userId, 1, 20, Arg.Any<CancellationToken>())
                .Returns(new List<LikedPostDto>());

            var sut = new GetLikedPostsHandler(_repo, HttpContextFixture.CreateAuthenticated(userId));
            var result = await sut.Handle(new GetLikedPostsQuery(1, 20), default);

            result.IsSuccess.Should().BeTrue();
            result.Value!.Items.Should().BeEmpty();
            result.Value.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldPassPageParams_ToRepository()
        {
            const string userId = "user-1";
            _repo.CountLikedPostAsync(userId, Arg.Any<CancellationToken>()).Returns(0);
            _repo.GetLikePostsAsync(userId, 3, 10, Arg.Any<CancellationToken>())
                .Returns(new List<LikedPostDto>());

            var sut = new GetLikedPostsHandler(_repo, HttpContextFixture.CreateAuthenticated(userId));
            await sut.Handle(new GetLikedPostsQuery(3, 10), default);

            await _repo.Received(1).GetLikePostsAsync(userId, 3, 10, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Authenticated_ShouldMarkAllItems_AsIsLikedTrue()
        {
            const string userId = "user-1";
            var dtos = Enumerable.Range(1, 3).Select(i => new LikedPostDto
            {
                PostId = i,
                Content = $"post {i}",
                CreatedAt = DateTime.UtcNow,
                AuthorUserName = "u",
                AuthorIdentification = "u_ident"
            }).ToList();

            _repo.CountLikedPostAsync(userId, Arg.Any<CancellationToken>()).Returns(3);
            _repo.GetLikePostsAsync(userId, 1, 20, Arg.Any<CancellationToken>()).Returns(dtos);

            var sut = new GetLikedPostsHandler(_repo, HttpContextFixture.CreateAuthenticated(userId));
            var result = await sut.Handle(new GetLikedPostsQuery(1, 20), default);

            result.Value!.Items.Should().AllSatisfy(p => p.IsLiked.Should().BeTrue());
        }
    }
}
