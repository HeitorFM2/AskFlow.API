using AskFlow.Application.Comments.Handlers;
using AskFlow.Application.Comments.Queries;
using AskFlow.Domain.Entities;
using AskFlow.Domain.Interfaces;
using AskFlow.Tests.Common.Builders;

namespace AskFlow.Tests.Application.Comments.Handlers
{
    public class GetCommentRepliesHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldMapReplies_AndCountNestedReplies()
        {
            var user = new UserBuilder().WithIdentification("bob").Build();
            var r1 = new CommentBuilder().WithId(11).WithParentCommentId(10).WithUser(user).Build();

            var repo = Substitute.For<ICommentRepository>();
            repo.CountRepliesAsync(10, Arg.Any<CancellationToken>()).Returns(1);
            repo.GetRepliesAsync(10, 1, 20, Arg.Any<CancellationToken>())
                .Returns(new List<Comment> { r1 });
            repo.GetReplyCountsAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
                .Returns(new Dictionary<int, int> { [11] = 2 });

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
