using AskFlow.Domain.Entities;
using AskFlow.Tests.Common.Builders;

namespace AskFlow.Tests.Domain
{
    public class PostTests
    {
        [Fact]
        public void Constructor_ShouldSetProperties_AndDefaults()
        {
            var userId = Guid.NewGuid().ToString();

            var post = new Post("conteúdo qualquer", userId);

            post.Content.Should().Be("conteúdo qualquer");
            post.UserId.Should().Be(userId);
            post.IsDeleted.Should().BeFalse();
            post.DeletedAt.Should().BeNull();
            post.UpdatedAt.Should().BeNull();
            post.Comments.Should().BeEmpty();
            post.Likes.Should().BeEmpty();
            post.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Builder_ShouldExposeNavigationCollections()
        {
            var user = new UserBuilder().Build();
            var comment = new CommentBuilder().WithUser(user).Build();
            var like = new LikeBuilder().WithUser(user).Build();

            var post = new PostBuilder()
                .WithUser(user)
                .WithComments(comment)
                .WithLikes(like)
                .Build();

            post.Comments.Should().ContainSingle().Which.Should().Be(comment);
            post.Likes.Should().ContainSingle().Which.Should().Be(like);
            post.User.Should().Be(user);
        }

        [Fact]
        public void ParameterlessConstructor_ShouldInitialize_DefaultValues()
        {
            var post = (Post)Activator.CreateInstance(typeof(Post), nonPublic: true)!;

            post.Should().NotBeNull();
            post.Id.Should().Be(0);
            post.Content.Should().BeEmpty();
            post.UserId.Should().BeEmpty();
            post.IsDeleted.Should().BeFalse();
            post.DeletedAt.Should().BeNull();
            post.UpdatedAt.Should().BeNull();
            post.Comments.Should().BeEmpty();
            post.Likes.Should().BeEmpty();
            post.CreatedAt.Should().Be(default);
        }
    }
}
