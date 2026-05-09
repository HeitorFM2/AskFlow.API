using AskFlow.Domain.Entities;
using Bogus;

namespace AskFlow.Tests.Common.Builders
{
    public class UserBuilder
    {
        private readonly Faker _faker = new("pt_BR");
        private string _id;
        private string _email;
        private string _userName;
        private string _identification;
        private DateTime _createdAt;

        public UserBuilder()
        {
            _id = Guid.NewGuid().ToString();
            _email = _faker.Internet.Email();
            _userName = _email;
            _identification = _faker.Internet.UserName();
            _createdAt = DateTime.UtcNow;
        }

        public UserBuilder WithId(string id)
        {
            _id = id;
            return this;
        }

        public UserBuilder WithEmail(string email)
        {
            _email = email;
            _userName = email;
            return this;
        }

        public UserBuilder WithIdentification(string identification)
        {
            _identification = identification;
            return this;
        }

        public UserBuilder WithCreatedAt(DateTime createdAt)
        {
            _createdAt = createdAt;
            return this;
        }

        public User Build() => new()
        {
            Id = _id,
            Email = _email,
            UserName = _userName,
            NormalizedEmail = _email.ToUpperInvariant(),
            NormalizedUserName = _userName.ToUpperInvariant(),
            Identification = _identification,
            CreatedAt = _createdAt
        };
    }
}
