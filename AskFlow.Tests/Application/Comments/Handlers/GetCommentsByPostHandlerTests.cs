using AskFlow.Application.Comments.Handlers;
using AskFlow.Application.Comments.Queries;
using AskFlow.Application.Comments.ViewModels;
using AskFlow.Application.Interfaces;
using AskFlow.Application.Users.ViewModels;

namespace AskFlow.Tests.Application.Comments.Handlers
{
    public class GetCommentsByPostHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldReturn_CommentsViewModel_WithReplyCounts()
        {
            var user = new UserDto { UserName = "alice", Identification = "alice" };
            var c1 = new CommentViewModel { Id = 1, Content = "c1", CreatedAt = DateTime.UtcNow, ReplyCount = 3, User = user };
            var c2 = new CommentViewModel { Id = 2, Content = "c2", CreatedAt = DateTime.UtcNow, ReplyCount = 0, User = user };

            var repo = Substitute.For<ICommentRepository>();
            repo.CountByPostAsync(10, Arg.Any<CancellationToken>()).Returns(2);
            repo.GetByPostAsync(10, 1, 20, Arg.Any<CancellationToken>())
                .Returns(new List<CommentViewModel> { c1, c2 });

            var sut = new GetCommentsByPostHandler(repo);
            var result = await sut.Handle(new GetCommentsByPostQuery(10), default);

            result.IsSuccess.Should().BeTrue();
            result.Value!.TotalCount.Should().Be(2);
            result.Value.Items.Should().HaveCount(2);
            result.Value.Items.First(i => i.Id == 1).ReplyCount.Should().Be(3);
            result.Value.Items.First(i => i.Id == 2).ReplyCount.Should().Be(0);
            result.Value.Items.First().User.Identification.Should().Be("alice");
        }
    }
}
