using AskFlow.Application.Comments.ViewModels;
using AskFlow.Application.Common;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Posts.Handlers;
using AskFlow.Application.Posts.Queries;
using AskFlow.Application.Posts.ViewModels;
using AskFlow.Application.Users.ViewModels;

namespace AskFlow.Tests.Application.Posts.Handlers
{
    public class GetByIdPostHandlerTests
    {
        [Fact]
        public async Task Handle_NotFound_ShouldReturnNotFound()
        {
            var repo = Substitute.For<IPostRepository>();
            repo.GetByIdAsync(1).Returns((PostViewModel?)null);
            var sut = new GetByIdPostHandler(repo);

            var result = await sut.Handle(new GetByIdPostQuery(1), default);

            result.Type.Should().Be(ResultType.NotFound);
            result.ErrorCode.Should().Be(ErrorCodes.PostNotFound);
        }

        [Fact]
        public async Task Handle_PostFound_ShouldReturnViewModel_IncludingComments()
        {
            var user = new UserViewModel { UserName = "user", Identification = "ident" };
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
