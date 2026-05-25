using AskFlow.Domain.Entities;
using Bogus;

namespace AskFlow.Tests.Common.Builders
{
    public class RefreshTokenBuilder
    {
        private readonly Faker _faker = new();
        private string _token;
        private User _user;
        private DateTime _expiresAt;
        private bool _revoke;

        public RefreshTokenBuilder()
        {
            _token = _faker.Random.AlphaNumeric(40);
            _user = new UserBuilder().Build();
            _expiresAt = DateTime.UtcNow.AddDays(7);
        }

        public RefreshTokenBuilder WithUser(User user)
        {
            _user = user;
            return this;
        }

        public RefreshTokenBuilder WithExpiresAt(DateTime expiresAt)
        {
            _expiresAt = expiresAt;
            return this;
        }

        public RefreshTokenBuilder Revoked()
        {
            _revoke = true;
            return this;
        }

        public RefreshToken Build()
        {
            var rt = new RefreshToken(_token, _user, _expiresAt);
            if (_revoke)
                rt.Revoke();
            return rt;
        }
    }
}
