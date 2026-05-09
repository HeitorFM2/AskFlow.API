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
            user.Posts.Should().BeEmpty();
            user.Comments.Should().BeEmpty();
            user.Likes.Should().BeEmpty();
        }
    }
}
