using AskFlow.Tests.Common.Builders;

namespace AskFlow.Tests.Domain
{
    public class LikeTests
    {
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
