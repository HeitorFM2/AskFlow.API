using AskFlow.Application.Interfaces;
using AskFlow.Application.Posts.Handlers;
using AskFlow.Application.Posts.Queries;
using AskFlow.Application.Posts.ViewModels;
using AskFlow.Application.Users.ViewModels;

namespace AskFlow.Tests.Application.Posts.Handlers
{
    public class GetAllPostsHandlerTests
    {
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

            var repo = Substitute.For<IPostRepository>();
            repo.CountAsync(Arg.Any<CancellationToken>()).Returns(1);
            repo.GetAllAsync(1, 20, Arg.Any<CancellationToken>())
                .Returns(new List<PostsViewModel> { item });

            var sut = new GetAllPostsHandler(repo);

            var result = await sut.Handle(new GetAllPostsQuery(1, 20), default);

            result.IsSuccess.Should().BeTrue();
            result.Value!.TotalCount.Should().Be(1);
            var returned = result.Value.Items.Should().ContainSingle().Subject;
            returned.Id.Should().Be(1);
            returned.Comments.Should().Be(1);
            returned.Likes.Should().Be(1);
            returned.User.Identification.Should().Be("user_a");
            result.Value.Page.Should().Be(1);
            result.Value.PageSize.Should().Be(20);
        }
    }
}
