using AskFlow.Domain.Entities;
using Bogus;

namespace AskFlow.Tests.Common.Builders
{
    public class CommentBuilder
    {
        private readonly Faker _faker = new("pt_BR");
        private int _id;
        private string _content;
        private string _userId;
        private int? _postId;
        private int? _parentCommentId;
        private User? _user;

        public CommentBuilder()
        {
            _id = _faker.Random.Int(1, 10000);
            _content = _faker.Lorem.Sentence(3);
            _userId = Guid.NewGuid().ToString();
            _postId = _faker.Random.Int(1, 10000);
        }

        public CommentBuilder WithId(int id)
        {
            _id = id;
            return this;
        }

        public CommentBuilder WithUserId(string userId)
        {
            _userId = userId;
            return this;
        }

        public CommentBuilder WithUser(User user)
        {
            _user = user;
            _userId = user.Id;
            return this;
        }

        public CommentBuilder WithPostId(int? postId)
        {
            _postId = postId;
            return this;
        }

        public CommentBuilder WithParentCommentId(int? parentCommentId)
        {
            _parentCommentId = parentCommentId;
            return this;
        }

        public Comment Build()
        {
            var comment = new Comment(_content, _userId, _postId, _parentCommentId)
            {
                Id = _id
            };
            if (_user is not null)
                comment.User = _user;
            return comment;
        }
    }
}
