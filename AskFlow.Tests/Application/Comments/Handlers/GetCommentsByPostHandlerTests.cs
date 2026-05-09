using AskFlow.Application.Comments.Handlers;
using AskFlow.Application.Comments.Queries;
using AskFlow.Domain.Entities;
using AskFlow.Domain.Interfaces;
using AskFlow.Tests.Common.Builders;

namespace AskFlow.Tests.Application.Comments.Handlers
{
    public class GetCommentsByPostHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldMapComments_AndIncludeReplyCounts()
        {
            var user = new UserBuilder().WithIdentification("alice").Build();
            var c1 = new CommentBuilder().WithId(1).WithUser(user).Build();
            var c2 = new CommentBuilder().WithId(2).WithUser(user).Build();

            var repo = Substitute.For<ICommentRepository>();
            repo.CountByPostAsync(10, Arg.Any<CancellationToken>()).Returns(2);
            repo.GetByPostAsync(10, 1, 20, Arg.Any<CancellationToken>())
                .Returns(new List<Comment> { c1, c2 });
            repo.GetReplyCountsAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
                .Returns(new Dictionary<int, int> { [1] = 3 });

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
