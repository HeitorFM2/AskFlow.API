using AskFlow.Tests.Common.Builders;

namespace AskFlow.Tests.Domain
{
    public class UserTests
    {
        [Fact]
        public void Builder_ShouldProduceUser_WithRelationsEmpty()
        {
            var user = new UserBuilder()
                .WithEmail("teste@askflow.com")
                .WithIdentification("askflow_user")
                .Build();

            user.Email.Should().Be("teste@askflow.com");
            user.UserName.Should().Be("teste@askflow.com");
            user.Identification.Should().Be("askflow_user");
            user.AvatarUrl.Should().BeNull();
            user.Posts.Should().BeEmpty();
            user.Comments.Should().BeEmpty();
            user.Likes.Should().BeEmpty();
        }

        [Fact]
        public void AvatarUrl_ShouldBeSettable()
        {
            var user = new UserBuilder()
                .WithAvatarUrl("https://cdn.example.com/avatars/u.jpg")
                .Build();

            user.AvatarUrl.Should().Be("https://cdn.example.com/avatars/u.jpg");
        }
    }
}
