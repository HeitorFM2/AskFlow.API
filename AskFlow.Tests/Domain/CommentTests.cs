using AskFlow.Domain.Entities;

namespace AskFlow.Tests.Domain
{
    public class CommentTests
    {
        [Fact]
        public void Constructor_ShouldSetMandatoryProperties()
        {
            var userId = Guid.NewGuid().ToString();

            var comment = new Comment("oi", userId, postId: 10);

            comment.Content.Should().Be("oi");
            comment.UserId.Should().Be(userId);
            comment.PostId.Should().Be(10);
            comment.ParentCommentId.Should().BeNull();
            comment.Replies.Should().BeEmpty();
            comment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Constructor_WithParent_ShouldStoreParentId()
        {
            var comment = new Comment("resposta", "user-1", postId: 1, parentCommentId: 99);

            comment.ParentCommentId.Should().Be(99);
        }

        [Fact]
        public void ParameterlessConstructor_ShouldInitialize_DefaultValues()
        {
            var comment = (Comment)Activator.CreateInstance(typeof(Comment), nonPublic: true)!;

            comment.Should().NotBeNull();
            comment.Id.Should().Be(0);
            comment.Content.Should().BeEmpty();
            comment.UserId.Should().BeEmpty();
            comment.PostId.Should().BeNull();
            comment.ParentCommentId.Should().BeNull();
            comment.ParentComment.Should().BeNull();
            comment.Replies.Should().BeEmpty();
            comment.CreatedAt.Should().Be(default);
        }
    }
}
