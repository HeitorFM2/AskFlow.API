using AskFlow.Domain.Entities;
using AskFlow.Tests.Common.Builders;

namespace AskFlow.Tests.Domain
{
    public class LikeTests
    {
        [Fact]
        public void Constructor_ShouldSet_PostId_And_UserId()
        {
            var user = new UserBuilder().Build();
            var post = new PostBuilder().WithUser(user).Build();
            const int postId = 7;

            var like = new Like(postId, user.Id) { User = user, Post = post };

            like.PostId.Should().Be(postId);
            like.UserId.Should().Be(user.Id);
        }

        [Fact]
        public void Constructor_ShouldLeave_Id_And_CreatedAt_AsDefault()
        {
            var user = new UserBuilder().Build();
            var post = new PostBuilder().WithUser(user).Build();

            var like = new Like(1, user.Id) { User = user, Post = post };

            like.Id.Should().Be(0);
            like.CreatedAt.Should().Be(default);
        }

        [Fact]
        public void PrivateConstructor_ShouldInitialize_DefaultValues()
        {
            var like = (Like)Activator.CreateInstance(typeof(Like), nonPublic: true)!;

            like.Id.Should().Be(0);
            like.PostId.Should().BeNull();
            like.UserId.Should().BeEmpty();
            like.CreatedAt.Should().Be(default);
        }

        [Fact]
        public void Builder_ShouldProduce_ValidLike()
        {
            var user = new UserBuilder().Build();
            var post = new PostBuilder().WithUser(user).Build();

            var like = new LikeBuilder()
                .WithId(42)
                .WithUser(user)
                .WithPost(post)
                .Build();

            like.Id.Should().Be(42);
            like.User.Should().Be(user);
            like.UserId.Should().Be(user.Id);
            like.Post.Should().Be(post);
        }
    }
}
