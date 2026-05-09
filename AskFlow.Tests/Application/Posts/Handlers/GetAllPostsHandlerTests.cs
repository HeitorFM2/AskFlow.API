using AskFlow.Application.Posts.Handlers;
using AskFlow.Application.Posts.Queries;
using AskFlow.Domain.Entities;
using AskFlow.Domain.Interfaces;
using AskFlow.Tests.Common.Builders;

namespace AskFlow.Tests.Application.Posts.Handlers
{
    public class GetAllPostsHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldMap_PostsToViewModel_WithCounts()
        {
            var user = new UserBuilder().WithEmail("a@b.com").WithIdentification("user_a").Build();
            var post = new PostBuilder()
                .WithId(1)
                .WithUser(user)
                .WithComments(new CommentBuilder().WithUser(user).Build())
                .WithLikes(new LikeBuilder().WithUser(user).Build())
                .Build();

            var repo = Substitute.For<IPostRepository>();
            repo.CountAsync(Arg.Any<CancellationToken>()).Returns(1);
            repo.GetAllAsync(1, 20, Arg.Any<CancellationToken>())
                .Returns(new List<Post> { post });

            var sut = new GetAllPostsHandler(repo);

            var result = await sut.Handle(new GetAllPostsQuery(1, 20), default);

            result.IsSuccess.Should().BeTrue();
            result.Value!.TotalCount.Should().Be(1);
            var item = result.Value.Items.Should().ContainSingle().Subject;
            item.Id.Should().Be(1);
            item.Comments.Should().Be(1);
            item.Likes.Should().Be(1);
            item.User.Identification.Should().Be("user_a");
            result.Value.Page.Should().Be(1);
            result.Value.PageSize.Should().Be(20);
        }
    }
}
