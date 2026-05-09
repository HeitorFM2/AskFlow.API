using AskFlow.Domain.Entities;

namespace AskFlow.Tests.Common.Builders
{
    public class LikeBuilder
    {
        private int _id = 1;
        private DateTime _createdAt = DateTime.UtcNow;
        private User? _user;
        private Post? _post;

        public LikeBuilder WithId(int id)
        {
            _id = id;
            return this;
        }

        public LikeBuilder WithUser(User user)
        {
            _user = user;
            return this;
        }

        public LikeBuilder WithPost(Post post)
        {
            _post = post;
            return this;
        }

        public Like Build()
        {
            var user = _user ?? new UserBuilder().Build();
            var post = _post ?? new PostBuilder().WithUser(user).Build();
            return new Like
            {
                Id = _id,
                CreatedAt = _createdAt,
                User = user,
                UserId = user.Id,
                Post = post
            };
        }
    }
}
