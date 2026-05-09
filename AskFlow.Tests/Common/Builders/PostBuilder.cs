using AskFlow.Domain.Entities;
using Bogus;

namespace AskFlow.Tests.Common.Builders
{
    public class PostBuilder
    {
        private readonly Faker _faker = new("pt_BR");
        private int _id;
        private string _content;
        private string _userId;
        private User? _user;
        private List<Comment> _comments = new();
        private List<Like> _likes = new();

        public PostBuilder()
        {
            _id = _faker.Random.Int(1, 10000);
            _content = _faker.Lorem.Sentence(5);
            _userId = Guid.NewGuid().ToString();
        }

        public PostBuilder WithId(int id)
        {
            _id = id;
            return this;
        }

        public PostBuilder WithContent(string content)
        {
            _content = content;
            return this;
        }

        public PostBuilder WithUserId(string userId)
        {
            _userId = userId;
            return this;
        }

        public PostBuilder WithUser(User user)
        {
            _user = user;
            _userId = user.Id;
            return this;
        }

        public PostBuilder WithComments(params Comment[] comments)
        {
            _comments = comments.ToList();
            return this;
        }

        public PostBuilder WithLikes(params Like[] likes)
        {
            _likes = likes.ToList();
            return this;
        }

        public Post Build()
        {
            var post = new Post(_content, _userId)
            {
                Id = _id
            };
            if (_user is not null)
                post.User = _user;
            foreach (var c in _comments)
                post.Comments.Add(c);
            foreach (var l in _likes)
                post.Likes.Add(l);
            return post;
        }
    }
}
