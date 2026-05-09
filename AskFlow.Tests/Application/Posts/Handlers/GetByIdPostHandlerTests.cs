using AskFlow.Application.Common;
using AskFlow.Application.Posts.Handlers;
using AskFlow.Application.Posts.Queries;
using AskFlow.Domain.Entities;
using AskFlow.Domain.Interfaces;
using AskFlow.Tests.Common.Builders;

namespace AskFlow.Tests.Application.Posts.Handlers
{
    public class GetByIdPostHandlerTests
    {
        [Fact]
        public async Task Handle_NotFound_ShouldReturnNotFound()
        {
            var repo = Substitute.For<IPostRepository>();
            repo.GetByIdAsync(1).Returns((Post?)null);
            var sut = new GetByIdPostHandler(repo);

            var result = await sut.Handle(new GetByIdPostQuery(1), default);

            result.Type.Should().Be(ResultType.NotFound);
        }

        [Fact]
        public async Task Handle_PostFound_ShouldMapToViewModel_IncludingComments()
        {
            var user = new UserBuilder().WithIdentification("ident").Build();
            var reply = new CommentBuilder().WithUser(user).Build();
            var comment = new CommentBuilder()
                .WithId(20)
                .WithUser(user)
                .WithReplies(reply)
                .Build();
            var post = new PostBuilder()
                .WithId(7)
                .WithUser(user)
                .WithComments(comment)
                .WithLikes(new LikeBuilder().WithUser(user).Build())
                .Build();

            var repo = Substitute.For<IPostRepository>();
            repo.GetByIdAsync(7).Returns(post);
            var sut = new GetByIdPostHandler(repo);

            var result = await sut.Handle(new GetByIdPostQuery(7), default);

            result.IsSuccess.Should().BeTrue();
            result.Value!.Id.Should().Be(7);
            result.Value.Likes.Should().Be(1);
            result.Value.Comments.Should().ContainSingle().Which.ReplyCount.Should().Be(1);
            result.Value.User.Identification.Should().Be("ident");
        }
    }
}
