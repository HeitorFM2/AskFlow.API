using AskFlow.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AskFlow.Tests.Infrastructure.Services
{
    public class CurrentUserServiceTests
    {
        private static CurrentUserService CreateSut(HttpContext? httpContext)
        {
            var accessor = Substitute.For<IHttpContextAccessor>();
            accessor.HttpContext.Returns(httpContext);
            return new CurrentUserService(accessor);
        }

        private static HttpContext CreateHttpContext(IEnumerable<Claim> claims)
        {
            var httpContext = Substitute.For<HttpContext>();
            httpContext.User.Returns(new ClaimsPrincipal(new ClaimsIdentity(claims)));
            return httpContext;
        }

        [Fact]
        public void GetUserId_ShouldReturn_UserId_FromSubClaim()
        {
            const string userId = "user-abc-123";
            var sut = CreateSut(CreateHttpContext([new Claim("sub", userId)]));

            var result = sut.GetUserId();

            result.Should().Be(userId);
        }

        [Fact]
        public void GetUserId_ShouldReturn_UserId_FromNameIdentifierClaim_WhenSubIsAbsent()
        {
            const string userId = "user-abc-123";
            var sut = CreateSut(CreateHttpContext([new Claim(ClaimTypes.NameIdentifier, userId)]));

            var result = sut.GetUserId();

            result.Should().Be(userId);
        }

        [Fact]
        public void GetUserId_ShouldPrefer_SubClaim_OverNameIdentifier()
        {
            const string subId = "sub-id";
            const string nameId = "name-id";
            var sut = CreateSut(CreateHttpContext([
                new Claim("sub", subId),
                new Claim(ClaimTypes.NameIdentifier, nameId)
            ]));

            var result = sut.GetUserId();

            result.Should().Be(subId);
        }

        [Fact]
        public void GetUserId_ShouldReturn_Null_WhenHttpContextIsNull()
        {
            var sut = CreateSut(null);

            var result = sut.GetUserId();

            result.Should().BeNull();
        }

        [Fact]
        public void GetUserId_ShouldReturn_Null_WhenUserHasNoRelevantClaims()
        {
            var sut = CreateSut(CreateHttpContext([new Claim(ClaimTypes.Email, "a@b.com")]));

            var result = sut.GetUserId();

            result.Should().BeNull();
        }
    }
}
