using AskFlow.Application.Comments.Handlers;
using AskFlow.Application.Comments.Queries;
using AskFlow.Application.Comments.ViewModels;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Users.ViewModels;

namespace AskFlow.Tests.Application.Comments.Handlers
{
    public class GetCommentRepliesHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldReturn_RepliesViewModel_WithNestedReplyCounts()
        {
            var user = new UserDto { UserName = "bob", Identification = "bob" };
            var r1 = new CommentViewModel { Id = 11, Content = "reply", CreatedAt = DateTime.UtcNow, ParentCommentId = 10, ReplyCount = 2, User = user };

            var repo = Substitute.For<ICommentRepository>();
            repo.CountRepliesAsync(10, Arg.Any<CancellationToken>()).Returns(1);
            repo.GetRepliesAsync(10, 1, 20, Arg.Any<CancellationToken>())
                .Returns(new List<CommentViewModel> { r1 });

            var sut = new GetCommentRepliesHandler(repo);
            var result = await sut.Handle(new GetCommentRepliesQuery(10), default);

            result.IsSuccess.Should().BeTrue();
            result.Value!.TotalCount.Should().Be(1);
            result.Value.Items.Should().ContainSingle().Which.ReplyCount.Should().Be(2);
            result.Value.Items[0].ParentCommentId.Should().Be(10);
            result.Value.Items[0].User.Identification.Should().Be("bob");
        }
    }
}
