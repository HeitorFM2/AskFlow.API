using AskFlow.Infrastructure.Services;
using AskFlow.Infrastructure.Settings;
using AskFlow.Tests.Common.Builders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace AskFlow.Tests.Infrastructure.Services
{
    public class TokenServiceTests
    {
        private readonly JwtSettings _settings = new()
        {
            SecretKey = "this-is-a-very-long-secret-key-for-tests-1234567890",
            Issuer = "askflow",
            Audience = "askflow-clients",
            ExpiresInMinutes = 30,
            RefreshTokenExpiresInDays = 7
        };

        private TokenService CreateSut() => new(Options.Create(_settings));

        [Fact]
        public void RefreshTokenExpiresInDays_ShouldReflectSettings()
        {
            CreateSut().RefreshTokenExpiresInDays.Should().Be(7);
        }

        [Fact]
        public void GetAccessTokenExpiry_ShouldBe_AboutNowPlusExpiresInMinutes()
        {
            var expiry = CreateSut().GetAccessTokenExpiry();

            expiry.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(30), TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void GenerateAccessToken_ShouldProduce_ValidJwt_WithExpectedClaims()
        {
            var user = new UserBuilder().WithEmail("a@b.com").WithIdentification("ident").Build();

            var token = CreateSut().GenerateAccessToken(user);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            jwt.Issuer.Should().Be(_settings.Issuer);
            jwt.Audiences.Should().Contain(_settings.Audience);
            jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "a@b.com");
            jwt.Claims.Should().Contain(c => c.Type == "identification" && c.Value == "ident");
            jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id);
            jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
        }

        [Fact]
        public void GenerateAccessToken_Token_ShouldValidate_AgainstSecretKey()
        {
            var user = new UserBuilder().Build();
            var token = CreateSut().GenerateAccessToken(user);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _settings.Issuer,
                ValidAudience = _settings.Audience,
                IssuerSigningKey = key
            };

            var act = () => handler.ValidateToken(token, validationParameters, out _);
            act.Should().NotThrow();
        }

        [Fact]
        public void GenerateRefreshToken_ShouldProduce_NonEmptyUniqueValues()
        {
            var sut = CreateSut();
            var t1 = sut.GenerateRefreshToken();
            var t2 = sut.GenerateRefreshToken();

            t1.Should().NotBeNullOrWhiteSpace();
            t1.Should().NotBe(t2);
        }
    }
}
